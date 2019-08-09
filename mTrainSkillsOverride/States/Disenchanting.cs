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

class Disenchanting : State
{
    public override string DisplayName
    {
        get { return "Disenchanting"; }
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
            if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && ObjectManager.Me.IsValid && !Conditions.IsAttackedAndCannotIgnore && mTrainSkillsOverrideSettings.CurrentSetting.DisenchantingItems && Skill.Has(SkillLine.Enchanting) && Main.disenchantTimer.IsReady)
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
        var enchantingSkill = Skill.GetValue(SkillLine.Enchanting);
        Logging.Write($"[TSO]: Checking for items to disenchant...", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
        foreach (var item in Bag.GetBagItem().Where(i => (mTrainSkillsOverrideSettings.CurrentSetting.DisenchantHighlevel ? i.GetItemInfo.ItemMinLevel < 999 : i.GetItemInfo.ItemMinLevel <= ObjectManager.Me.Level) && (mTrainSkillsOverrideSettings.CurrentSetting.DisenchantBlues ? i.GetItemInfo.ItemRarity >= 2 : i.GetItemInfo.ItemRarity == 2) && (i.GetItemInfo.ItemType == "Armor" || i.GetItemInfo.ItemType == "Weapon")))
        {
            var itemLvl = item.GetItemInfo.ItemMinLevel;
            var requiredSkill = 5 * (itemLvl % 5 == 0 ? itemLvl : (itemLvl += (5 - (itemLvl % 5)))) - 75;
            if (requiredSkill < enchantingSkill)
            {
                Logging.Write($"[TSO]: Item found - {item.Name}, {item.Type} | {item.GetItemInfo.ItemRarity}", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
                Main.Disenchant.Launch();
                Thread.Sleep(50);
                //Bag.PickupContainerItem(item.Entry);
                ItemsManager.UseItem((uint)item.Entry);
                Usefuls.WaitIsCasting();
            }
            else
            {
                Logging.Write($"[TSO]: Don't have the skill for - {item.Name}, {item.Type} | {item.GetItemInfo.ItemRarity}", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
            }
        }
        Logging.Write($"[TSO]: ...disenchanting checks completed.", Logging.LogType.Normal, System.Drawing.Color.SteelBlue);
        Main.disenchantTimer.Reset();
    }
}


