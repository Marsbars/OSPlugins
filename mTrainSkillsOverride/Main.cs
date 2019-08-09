using MarsSettingsGUI;
using robotManager;
using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using robotManager.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using wManager;
using wManager.Wow.Bot.States;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : wManager.Plugin.IPlugin
{
    public static bool needToTrainSpells;
    public static List<SkillLine> myPrimaryProfs = new List<SkillLine>();
    public static List<SkillLine> mySecondaryProfs = new List<SkillLine>();
    public static WoWClass wowClass = new WoWClass();
    public static Npc.NpcType classType = new Npc.NpcType();
    private bool hasAddedState;
    public static Timer disenchantTimer = new Timer();
    public static Spell Disenchant;
    public void Dispose()
    {
        robotManager.Events.FiniteStateMachineEvents.OnBeforeCheckIfNeedToRunState -= FiniteStateMachineEvents_OnBeforeCheckIfNeedToRunState;
    }

    public void Initialize()
    {
        mTrainSkillsOverrideSettings.Load();
        LoadVariables();

        wManager.wManagerSetting.CurrentSetting.TrainNewSkills = false;
        robotManager.Events.FiniteStateMachineEvents.OnBeforeCheckIfNeedToRunState += FiniteStateMachineEvents_OnBeforeCheckIfNeedToRunState;
        EventsLua.AttachEventLua((LuaEventsId)Enum.Parse(typeof(LuaEventsId), "PLAYER_LEVEL_UP"), m => OnLevelUp());
        disenchantTimer = new Timer(1000 * mTrainSkillsOverrideSettings.CurrentSetting.DisenchantingTimer);
        disenchantTimer.Reset();
    }

    private void LoadVariables()
    {
        wowClass = ObjectManager.Me.WowClass;
        myPrimaryProfs = Professions.Primary.Where(x => Skill.Has(x)).ToList();
        mySecondaryProfs = Professions.Secondary.Where(x => Skill.Has(x)).ToList();
        switch (wowClass)
        {
            case WoWClass.Warrior:
                classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "WarriorTrainer");
                break;
            case WoWClass.Paladin:
                classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "PaladinTrainer");
                break;
            case WoWClass.Hunter:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "HunterTrainer");
                break;
            case WoWClass.Rogue:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "RogueTrainer");
                break;
            case WoWClass.Priest:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "PriestTrainer");
                break;
            case WoWClass.DeathKnight:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "DeathKnightTrainer");
                break;
            case WoWClass.Shaman:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "ShamanTrainer");
                break;
            case WoWClass.Mage:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "MageTrainer");
                break;
            case WoWClass.Warlock:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "WarlockTrainer");
                break;
            case WoWClass.Monk:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "MonkTrainer");
                break;
            case WoWClass.Druid:
                classType = classType = (Npc.NpcType)Enum.Parse(typeof(Npc.NpcType), "DruidTrainer");
                break;
        }
    }

    private void FiniteStateMachineEvents_OnBeforeCheckIfNeedToRunState(Engine engine, State state, CancelEventArgs cancelable)
    {

        if (engine.States.Any(s => s.GetType() == typeof(Trainers)) && engine.States.Count(t => t.GetType() == typeof(TrainClassSpells)) == 0 && !hasAddedState)
        {
            var prio = engine.States.Find(s => s.GetType() == typeof(Trainers)).Priority;

            if (mTrainSkillsOverrideSettings.CurrentSetting.DisenchantingItems)
            {
                engine.AddState(new Disenchanting() { Priority = prio });
                Logging.Write($"[TSO]: Disenchanting state added.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
                Disenchant = new Spell("Disenchant");
            }

            if (mTrainSkillsOverrideSettings.CurrentSetting.TrainPrimary || mTrainSkillsOverrideSettings.CurrentSetting.TrainSecondary)
            {
                engine.AddState(new TrainProfessions() { Priority = prio });
                Logging.Write($"[TSO]: Profession Training state added.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
            }

            if (mTrainSkillsOverrideSettings.CurrentSetting.EnableSpellTraining)
            {
                engine.AddState(new TrainClassSpells() { Priority = prio + 1 });
                engine.RemoveStateByName("Trainers");
                Logging.Write($"[TSO]: Class Training state added.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
            }
            engine.States.Sort();
            hasAddedState = true;
        }
    }

    private void OnLevelUp()
    {
        needToTrainSpells = true;
    }

    public void Settings()
    {
        mTrainSkillsOverrideSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mTrainSkillsOverrideSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mTrainSkillsOverrideSettings.CurrentSetting.Save();
    }

    
}
[Serializable]
public class mTrainSkillsOverrideSettings : Settings
{

    [Setting]
    [Category("Spells")]
    [DisplayName("Enable Spell Override")]
    [Description("1.Toggle to disable the class spell trainer override")]
    public bool EnableSpellTraining { get; set; }

    [Setting]
    [Category("Spells")]
    [DisplayName("Spells to buy")]
    [Description("A list of the spells the bot should by at the trainer")]
    public List<string> SpellsToBuy { get; set; }

    [Setting]
    [Category("Spells")]
    [DisplayName("Levels to go train on")]
    [Description("A list of the levels the bot should go to the trainer")]
    public List<int> LevelsToTrain { get; set; }

    [Setting]
    [Category("Settings")]
    [DisplayName("Minimum silver")]
    [Description("The amount of silver the bot must have to go to trainers")]
    public long MinimumSilver { get; set; }

    [Setting]
    [Category("Professions")]
    [DisplayName("Train primary professions")]
    [Description("")]
    [Order(1)]
    public bool TrainPrimary { get; set; }

    [Setting]
    [Category("Professions")]
    [DisplayName("Train secondary professions")]
    [Description("")]
    [Order(2)]
    public bool TrainSecondary { get; set; }

    [Setting]
    [Category("Professions")]
    [DisplayName("Disenchanting")]
    [Description("")]
    [Order(3)]
    public bool DisenchantingItems { get; set; }

    [Setting]
    [Category("Professions")]
    [DisplayName("Disenchant blues?")]
    [Description("")]
    [Order(4)]
    public bool DisenchantBlues { get; set; }

    [Setting]
    [Category("Professions")]
    [DisplayName("Disenchant high level?")]
    [Description("This will disenchant items with a higher required level than your characters level")]
    [Order(5)]
    public bool DisenchantHighlevel { get; set; }

    [Setting]
    [Category("Professions")]
    [DisplayName("Disenchant Timer")]
    [Description("Run disenchant checks every X secs")]
    [Order(6)]
    public long DisenchantingTimer { get; set; }

    private mTrainSkillsOverrideSettings()
    {
        SpellsToBuy = new List<string>();
        LevelsToTrain = new List<int>();
        EnableSpellTraining = true;
        TrainPrimary = true;
        TrainSecondary = true;
        MinimumSilver = 0;
        DisenchantingItems = true;
        DisenchantBlues = false;
        DisenchantHighlevel = false;
        DisenchantingTimer = 10;
    }

    public static mTrainSkillsOverrideSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("mTrainSkillsOverride", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("mTrainSkillsOverrideSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("mTrainSkillsOverride", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =
                    Load<mTrainSkillsOverrideSettings>(AdviserFilePathAndName("mTrainSkillsOverride", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new mTrainSkillsOverrideSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("mTrainSkillsOverrideSettings > Load(): " + e);
        }
        return false;
    }
}