using robotManager;
using robotManager.Helpful;
using robotManager.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using wManager;
using wManager.Wow.Class;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : wManager.Plugin.IPlugin
{
    public System.Timers.Timer buffCheckTimer = new System.Timers.Timer() { Interval = 5000 };
    public System.Timers.Timer playerBlacklistClear = new System.Timers.Timer() { Interval = 1000 * 60 };
    public List<WoWUnit> playerBlacklist = new List<WoWUnit>();
    public List<Spell> BuffSpells = new List<Spell>();
    public string botState = "";
    public void Dispose()
    {
        EventsLuaWithArgs.OnEventsLuaWithArgs -= EventsLuaWithArgs_OnEventsLuaWithArgs;
        robotManager.Events.FiniteStateMachineEvents.OnRunState -= FiniteStateMachineEvents_OnRunState;
        buffCheckTimer.Dispose();
        playerBlacklistClear.Dispose();
        mlog("Stopped.");
    }

    public void Initialize()
    {
        mBufferSettings.Load();
        foreach (var spell in mBufferSettings.CurrentSetting.BuffSpells)
        {
            Spell s = new Spell(spell);
            if (s.KnownSpell)
            {
                mlog($"Adding {s.Name} to buffing spell list.");
            }
            else { mlog($"Spell '{s.Name}' is either unknown or is not on your action bar."); }
        }
        robotManager.Events.FiniteStateMachineEvents.OnRunState += FiniteStateMachineEvents_OnRunState;
        EventsLuaWithArgs.OnEventsLuaWithArgs += EventsLuaWithArgs_OnEventsLuaWithArgs;
        buffCheckTimer.Elapsed += BuffCheckTimer_Elapsed;
        buffCheckTimer.Start();
        playerBlacklistClear.Elapsed += PlayerBlacklistClear_Elapsed;
        playerBlacklistClear.Start();
    }

    private void BuffCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (ObjectManager.Me.ManaPercentage >= mBufferSettings.CurrentSetting.MinMana && !ObjectManager.Me.InCombat && !Fight.InFight && botState != "Regeneration")
        {
            foreach (var spell in BuffSpells)
            {
                WoWPlayer p = null;
                if (spell.Name.Contains("wisdom") || spell.Name.Contains("intellect"))
                {
                    p = FindPlayerToBuff(spell.Name, true);
                }
                else
                {
                    p = FindPlayerToBuff(spell.Name, false);
                }
                if (p != null)
                {
                    mlog($"Casting '{spell.Name} on '{p.Name}'.");
                    Interact.InteractGameObject(p.GetBaseAddress);
                    SpellManager.CastSpellByNameLUA(spell.Name);
                }
            }
        }
    }

    private void FiniteStateMachineEvents_OnRunState(robotManager.FiniteStateMachine.Engine engine, robotManager.FiniteStateMachine.State state, CancelEventArgs cancelable)
    {
        botState = state.DisplayName;
    }

    private WoWPlayer FindPlayerToBuff(string spellName, bool casterBuff)
    {
        WoWPlayer a = null;
        try
        {
            if (casterBuff)
            {
                return a = ObjectManager.GetObjectWoWPlayer().Where(x => x.IsAlive && x.IsValid && x.PlayerFaction == ObjectManager.Me.PlayerFaction && x.GetDistance <= 20 && !x.HaveBuff(spellName) && !playerBlacklist.Contains(x) && x.PowerType == (PowerType)Enum.Parse(typeof(PowerType), "Mana")).OrderBy(y => y.GetDistance).First();
            }
            else
            {
                return a = ObjectManager.GetObjectWoWPlayer().Where(x => x.IsAlive && x.IsValid && x.PlayerFaction == ObjectManager.Me.PlayerFaction && x.GetDistance <= 20 && !x.HaveBuff(spellName) && !playerBlacklist.Contains(x)).OrderBy(y => y.GetDistance).First();
            }
        }
        catch (Exception e) { mlog("Error: " + e.ToString()); }
        return null;
    }

    private void EventsLuaWithArgs_OnEventsLuaWithArgs(LuaEventsId id, List<string> args)
    {
        if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "UI_ERROR_MESSAGE"))
        {
            if (args.First().Contains("more powerful spell"))
            {
                playerBlacklist.Add(ObjectManager.Target);
            }
        }
    }
    private void PlayerBlacklistClear_Elapsed(object sender, ElapsedEventArgs e)
    {
        playerBlacklist.Clear();
    }

    private void mlog(string msg)
    {
        Logging.Write($"[mBuffer]: {msg}", Logging.LogType.Normal, System.Drawing.Color.DodgerBlue);
    }

    public void Settings()
    {
        mBufferSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mBufferSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mBufferSettings.CurrentSetting.Save();
    }

    [Serializable]
    public class mBufferSettings : Settings
    {

        [Setting]
        [Category("Settings")]
        [DisplayName("Config")]
        [Description("Description")]
        public List<string> BuffSpells { get; set; }

        [Setting]        
        [Category("Config")]
        [DisplayName("Minimum mana percentage")]
        [Description("Minimum amount of mana required before buffing")]
        public int MinMana { get; set; }

        private mBufferSettings()
        {
            MinMana = 60;
        }

        public static mBufferSettings CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("mBuffer", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("mBufferSettings > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("mBuffer", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<mBufferSettings>(AdviserFilePathAndName("mBuffer", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new mBufferSettings();
            }
            catch (Exception e)
            {
                Logging.WriteError("mBufferSettings > Load(): " + e);
            }
            return false;
        }
    }
}
