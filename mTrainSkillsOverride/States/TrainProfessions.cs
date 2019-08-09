using robotManager.FiniteStateMachine;
using robotManager.Helpful;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wManager.Wow.Bot.Tasks;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

class TrainProfessions : State
{
    public override string DisplayName
    {
        get { return "GoTo train professions"; }
    }
    public override int Priority
    {
        get { return _priority; }
        set { _priority = value; }
    }
    private int _priority;

    //private List<SkillLine> profsToTrain = new List<SkillLine>();
    private Npc.NpcType type = Npc.NpcType.None;
    private Npc npc = new Npc();
    private List<Npc> profNpcs = new List<Npc>();

    public override bool NeedToRun
    {
        get
        {
            if (ObjectManager.Me.GetMoneyCopper * 100 >= mTrainSkillsOverrideSettings.CurrentSetting.MinimumSilver && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.IsValid && !Conditions.IsAttackedAndCannotIgnore)
            {

                if (Main.myPrimaryProfs.Any(x => Skill.GetValue(x) >= Skill.GetMaxValue(x) - 25 && Skill.GetMaxValue(x) < 300) && mTrainSkillsOverrideSettings.CurrentSetting.TrainPrimary)
                {

                    foreach (var t in Main.myPrimaryProfs.Where(x => Skill.GetValue(x) >= Skill.GetMaxValue(x) - 25 && Skill.GetMaxValue(x) < 300))
                    {
                        switch (t)
                        {
                            case SkillLine.Alchemy:
                                type = Npc.NpcType.AlchemyTrainer;
                                break;
                            case SkillLine.Blacksmithing:
                                type = Npc.NpcType.BlacksmithingTrainer;
                                break;
                            case SkillLine.Enchanting:
                                type = Npc.NpcType.EnchantingTrainer;
                                break;
                            case SkillLine.Engineering:
                                type = Npc.NpcType.EngineeringTrainer;
                                break;
                            case SkillLine.Herbalism:
                                type = Npc.NpcType.HerbalismTrainer;
                                break;
                            case SkillLine.Inscription:
                                type = Npc.NpcType.InscriptionTrainer;
                                break;
                            case SkillLine.Jewelcrafting:
                                type = Npc.NpcType.JewelcraftingTrainer;
                                break;
                            case SkillLine.Leatherworking:
                                type = Npc.NpcType.LeatherworkingTrainer;
                                break;
                            case SkillLine.Mining:
                                type = Npc.NpcType.MiningTrainer;
                                break;
                            case SkillLine.Skinning:
                                type = Npc.NpcType.SkinningTrainer;
                                break;
                            case SkillLine.Tailoring:
                                type = Npc.NpcType.TailoringTrainer;
                                break;
                            case SkillLine.Archaeology:
                                type = Npc.NpcType.ArchaeologyTrainers;
                                break;
                            case SkillLine.Cooking:
                                type = Npc.NpcType.CookingTrainer;
                                break;
                            case SkillLine.FirstAid:
                                type = Npc.NpcType.FirstAidTrainer;
                                break;
                            case SkillLine.Fishing:
                                type = Npc.NpcType.FishingTrainer;
                                break;
                        }
                        if (type != Npc.NpcType.None)
                        {
                            npc = NpcDB.GetNpcNearby(type);
                        }
                        if (npc.Entry > 0)
                        {
                            Logging.Write($"[TSO]: Trainer found - {npc.Name}", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
                            profNpcs.Add(npc);
                        }
                        else
                        {
                            Logging.Write($"[TSO]: No Trainer found for profession: {t}.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
                        }
                    }
                }
                if (Main.mySecondaryProfs.Any(x => Skill.GetValue(x) >= Skill.GetMaxValue(x) - 25 && Skill.GetMaxValue(x) < 150) && mTrainSkillsOverrideSettings.CurrentSetting.TrainSecondary)
                {
                    foreach (var t in Main.mySecondaryProfs.Where(x => Skill.GetValue(x) >= Skill.GetMaxValue(x) - 25 && Skill.GetMaxValue(x) < 150))
                    {
                        switch (t)
                        {
                            case SkillLine.Alchemy:
                                type = Npc.NpcType.AlchemyTrainer;
                                break;
                            case SkillLine.Blacksmithing:
                                type = Npc.NpcType.BlacksmithingTrainer;
                                break;
                            case SkillLine.Enchanting:
                                type = Npc.NpcType.EnchantingTrainer;
                                break;
                            case SkillLine.Engineering:
                                type = Npc.NpcType.EngineeringTrainer;
                                break;
                            case SkillLine.Herbalism:
                                type = Npc.NpcType.HerbalismTrainer;
                                break;
                            case SkillLine.Inscription:
                                type = Npc.NpcType.InscriptionTrainer;
                                break;
                            case SkillLine.Jewelcrafting:
                                type = Npc.NpcType.JewelcraftingTrainer;
                                break;
                            case SkillLine.Leatherworking:
                                type = Npc.NpcType.LeatherworkingTrainer;
                                break;
                            case SkillLine.Mining:
                                type = Npc.NpcType.MiningTrainer;
                                break;
                            case SkillLine.Skinning:
                                type = Npc.NpcType.SkinningTrainer;
                                break;
                            case SkillLine.Tailoring:
                                type = Npc.NpcType.TailoringTrainer;
                                break;
                            case SkillLine.Archaeology:
                                type = Npc.NpcType.ArchaeologyTrainers;
                                break;
                            case SkillLine.Cooking:
                                type = Npc.NpcType.CookingTrainer;
                                break;
                            case SkillLine.FirstAid:
                                type = Npc.NpcType.FirstAidTrainer;
                                break;
                            case SkillLine.Fishing:
                                type = Npc.NpcType.FishingTrainer;
                                break;
                        }
                        if (type != Npc.NpcType.None)
                        {
                            npc = NpcDB.GetNpcNearby(type);
                        }
                        if (npc.Entry > 0)
                        {
                            Logging.Write($"[TSO]: Trainer found - {npc.Name}", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
                            profNpcs.Add(npc);
                        }
                        else
                        {
                            Logging.Write($"[TSO]: No Trainer found for profession: {t}.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
                        }
                    }
                }
                if (profNpcs.Count > 0)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public override List<State> NextStates
    {
        get { return new List<State>(); }
    }

    public override List<State> BeforeStates
    {
        get { return new List<State>(); }
    }

    public override void Run()
    {
        foreach (var n in profNpcs)
        {
            Logging.Write($"[TSO]: Trainer found - {npc.Name}, {npc.Type}", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
            if (GoToTask.ToPositionAndIntecractWith(npc))
            {
                TrainProfs();
                Thread.Sleep(200);
                TrainProfs();
                Thread.Sleep(200);
                Logging.Write($"[TSO]: Training profession completed.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
            }
        }
        profNpcs.Clear();
    }

    public void TrainProfs()
    {
        Lua.LuaDoString(@"SetTrainerServiceTypeFilter(""available"",1);
                            SetTrainerServiceTypeFilter(""unavailable"", 0);
                            SetTrainerServiceTypeFilter(""used"", 0); ");
        int numAvailableSpells = Lua.LuaDoString<int>("return GetNumTrainerServices()");
        if (numAvailableSpells > 0)
        {
            for (int i = 1; i < numAvailableSpells + 1; i++)
            {
                var spellName = Lua.LuaDoString<string>($"name, rank, category, expanded = GetTrainerServiceInfo({i}); return name;");
                if (spellName.Contains("Journeyman") || spellName.Contains("Expert") || spellName.Contains("Artisan"))
                {
                    if (ObjectManager.Me.GetMoneyCopper > Lua.LuaDoString<int>($"moneyCost, talentCost, professionCost = GetTrainerServiceCost({i}); return moneyCost"))
                    {
                        Lua.LuaDoString($"BuyTrainerService({i});");
                    }
                    else
                    {
                        Logging.Write($"[TSO]: Not enough money to buy {spellName}", Logging.LogType.Error, System.Drawing.Color.SteelBlue);
                    }
                }
            }
        }
    }
}


