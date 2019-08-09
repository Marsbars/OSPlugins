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

class TrainClassSpells : State
{
    public override string DisplayName
    {
        get { return "GoTo train class spells"; }
    }
    public override int Priority
    {
        get { return _priority; }
        set { _priority = value; }
    }
    private int _priority;

    public override bool NeedToRun
    {
        get
        {
            if (mTrainSkillsOverrideSettings.CurrentSetting.LevelsToTrain.Count == 0)
            {
                for (var i = 1; i <= 80; i++)
                {
                    if (i % 2 == 0)
                    {
                        mTrainSkillsOverrideSettings.CurrentSetting.LevelsToTrain.Add(i);
                    }
                }
            }
            if (ObjectManager.Me.GetMoneyCopper * 100 >= mTrainSkillsOverrideSettings.CurrentSetting.MinimumSilver && Main.needToTrainSpells && mTrainSkillsOverrideSettings.CurrentSetting.LevelsToTrain.Any(x => x == ObjectManager.Me.Level) && NpcDB.GetNpcNearby(Main.classType, ObjectManager.Me.PlayerFaction == "Horde" ? Npc.FactionType.Horde : Npc.FactionType.Alliance, (ContinentId)Usefuls.ContinentId, ObjectManager.Me.Position).Entry > 0 && Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.IsValid && !Conditions.IsAttackedAndCannotIgnore)
            {

                return true;
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
        Npc npc = new Npc();

        if (Main.classType != Npc.NpcType.None)
        {
            npc = NpcDB.GetNpcNearby(Main.classType, ObjectManager.Me.PlayerFaction == "Horde" ? Npc.FactionType.Horde : Npc.FactionType.Alliance, (ContinentId)Usefuls.ContinentId, ObjectManager.Me.Position);
        }
        if (npc.Entry > 0)
        {
            Logging.Write($"[TSO]: Trainer found - {npc.Name}", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
            if (GoToTask.ToPositionAndIntecractWith(npc))
            {
                TrainSpells();
                Thread.Sleep(200);
                TrainSpells();
                Thread.Sleep(200);
                TrainSpells();
                Thread.Sleep(200);
                TrainSpells();
                Thread.Sleep(200);
                Main.needToTrainSpells = false;
                Logging.Write($"[TSO]: Training class spells completed.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
            }
        }
        else
        {
            Main.needToTrainSpells = false;
        }

    }

    public void TrainSpells()
    {
        if (mTrainSkillsOverrideSettings.CurrentSetting.SpellsToBuy.Count == 0)
        {
            Trainer.TrainingSpell();
        }
        else
        {
            Lua.LuaDoString(@"SetTrainerServiceTypeFilter(""available"",1);
                            SetTrainerServiceTypeFilter(""unavailable"", 0);
                            SetTrainerServiceTypeFilter(""used"", 0); ");
            int numAvailableSpells = Lua.LuaDoString<int>("return GetNumTrainerServices()");
            for (int i = 0; i < numAvailableSpells; i++)
            {
                var spellName = Lua.LuaDoString<string>($"name, rank, category, expanded = GetTrainerServiceInfo({i}); return name;");
                if (mTrainSkillsOverrideSettings.CurrentSetting.SpellsToBuy.Contains(spellName))
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


