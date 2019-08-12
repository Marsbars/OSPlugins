using MarsSettingsGUI;
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
using wManager;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : wManager.Plugin.IPlugin
{
    public void Dispose()
    {
        EventsLuaWithArgs.OnEventsLuaWithArgs -= EventsLuaWithArgs_OnEventsLuaWithArgs;
        log("Disposed.");
    }

    public void Initialize()
    {
        mRequestHandlerSettings.Load();
        EventsLuaWithArgs.OnEventsLuaWithArgs += EventsLuaWithArgs_OnEventsLuaWithArgs;
        log("Initialized... Tracking LUA events.");
    }

    private void log(string msg)
    {
        Logging.Write("[RequestHandler]: " + msg, Logging.LogType.Normal, System.Drawing.Color.DarkCyan);
    }

    private void RandomWait()
    {
        int ms = new System.Random().Next((int)mRequestHandlerSettings.CurrentSetting.WaitMin, (int)mRequestHandlerSettings.CurrentSetting.WaitMax);
        System.Threading.Thread.Sleep(ms);
    }

    private void EventsLuaWithArgs_OnEventsLuaWithArgs(LuaEventsId id, List<string> args)
    {
        if (Conditions.InGameAndConnectedAndProductStartedNotInPause)
        {
            if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "START_LOOT_ROLL"))
            {
                if (mRequestHandlerSettings.CurrentSetting.LootR)
                {
                    RandomWait();
                    int rollType;
                    switch (mRequestHandlerSettings.CurrentSetting.LootRSetting)
                    {
                        case 0:
                            rollType = 1;
                            log("Rolling Need.");
                            break;
                        case 1:
                            rollType = 2;
                            log("Rolling Greed.");
                            break;
                        case 2:
                            rollType = 0;
                            log("Rolling Pass.");
                            break;
                        default:
                            rollType = 2;
                            log("Rolling Greed.");
                            break;
                    }
                    Lua.LuaDoString("RollOnLoot(" + args[0].ToString() + ", " + rollType.ToString() + ")");
                }
                return;
            }
            if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "READY_CHECK"))
            {
                log("Ready check started by '" + args[0] + "'.");
                if (Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();") && mRequestHandlerSettings.CurrentSetting.Rcheck)
                {
                    RandomWait();
                    if (mRequestHandlerSettings.CurrentSetting.PartyRAccept)
                    {
                        Lua.LuaDoString("StaticPopup1Button1:Click()");
                        log("Ready check accepted.");
                    }
                    else
                    {
                        Lua.LuaDoString("StaticPopup1Button2:Click()");
                        log("Ready check declined.");
                    }
                }
            }
            if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "RESURRECT_REQUEST"))
            {
                log("Rezz request from '" + args[0] + "'.");
                if (Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();") && mRequestHandlerSettings.CurrentSetting.RezzR)
                {
                    RandomWait();
                    if (mRequestHandlerSettings.CurrentSetting.RezzRAccept)
                    {
                        Lua.LuaDoString("StaticPopup1Button1:Click()");
                        log("Rezz request accepted.");
                    }
                    else
                    {
                        Lua.LuaDoString("StaticPopup1Button2:Click()");
                        log("Rezz request declined.");
                    }
                }
            }
            if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "DUEL_REQUESTED"))
            {
                log("Duel request from '" + args[0] + "'.");
                if (Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();") && mRequestHandlerSettings.CurrentSetting.DuelR)
                {
                    RandomWait();
                    if (mRequestHandlerSettings.CurrentSetting.DuelRAccept)
                    {
                        Lua.LuaDoString("StaticPopup1Button1:Click()");
                        log("Duel request accepted.");
                    }
                    else
                    {
                        Lua.LuaDoString("StaticPopup1Button2:Click()");
                        log("Duel request declined.");
                    }
                }
            }
            if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "TRADE_REQUEST"))
            {
                log("Trade request from '" + args[0] + "'.");
                if (Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();") && mRequestHandlerSettings.CurrentSetting.TradeR)
                {
                    RandomWait();
                    if (mRequestHandlerSettings.CurrentSetting.TradeRAccept)
                    {
                        Lua.LuaDoString("StaticPopup1Button1:Click()");
                        log("Trade request accepted.");
                    }
                    else
                    {
                        Lua.LuaDoString("StaticPopup1Button2:Click()");
                        log("Trade request declined.");
                    }
                }
            }
            if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "GUILD_INVITE_REQUEST"))
            {
                log("Guild request from '" + args[0] + "'.");
                if (Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();") && mRequestHandlerSettings.CurrentSetting.GuildR)
                {
                    RandomWait();
                    if (mRequestHandlerSettings.CurrentSetting.GuildRAccept)
                    {
                        Lua.LuaDoString("StaticPopup1Button1:Click()");
                        log("Guild request accepted.");
                    }
                    else
                    {
                        Lua.LuaDoString("StaticPopup1Button2:Click()");
                        log("Guild request declined.");
                    }
                }
            }
            if (id == (LuaEventsId)Enum.Parse(typeof(LuaEventsId), "PARTY_INVITE_REQUEST"))
            {
                log("Party request from '" + args[0] + "'.");

                if (Lua.LuaDoString<bool>("return StaticPopup1 and StaticPopup1:IsVisible();") && mRequestHandlerSettings.CurrentSetting.PartyR)
                {
                    RandomWait();
                    if (mRequestHandlerSettings.CurrentSetting.PartyRAccept)
                    {
                        Lua.LuaDoString("StaticPopup1Button1:Click()");
                        log("Party request accepted.");
                    }
                    else
                    {
                        Lua.LuaDoString("StaticPopup1Button2:Click()");
                        log("Party request declined.");
                    }
                }
            }
        }
    }


    public void Settings()
    {
        mRequestHandlerSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mRequestHandlerSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mRequestHandlerSettings.CurrentSetting.Save();
    }

    
}
[Serializable]
public class mRequestHandlerSettings : Settings
{

    [Setting]
    [DefaultValue(500)]
    [Category("Config")]
    [DisplayName("Min wait time in MS")]
    [Description("")]
    public long WaitMin { get; set; }

    [Setting]
    [DefaultValue(4500)]
    [Category("Config")]
    [DisplayName("Max wait time in MS")]
    [Description("")]
    public long WaitMax { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Party")]
    [DisplayName("Handle Party Requests")]
    [Description("")]
    public bool PartyR { get; set; }

    [Setting]
    [DefaultValue(false)]
    [Category("Party")]
    [DisplayName("Accept request?")]
    [Description("If true it will accept, if false it will decline")]
    public bool PartyRAccept { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Guild")]
    [DisplayName("Handle Guild Requests")]
    [Description("")]
    public bool GuildR { get; set; }

    [Setting]
    [DefaultValue(false)]
    [Category("Guild")]
    [DisplayName("Accept request?")]
    [Description("If true it will accept, if false it will decline")]
    public bool GuildRAccept { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Trade")]
    [DisplayName("Handle Trade Requests")]
    [Description("")]
    public bool TradeR { get; set; }

    [Setting]
    [DefaultValue(false)]
    [Category("Trade")]
    [DisplayName("Accept request?")]
    [Description("If true it will accept, if false it will decline")]
    public bool TradeRAccept { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Duel")]
    [DisplayName("Handle Duel Requests")]
    [Description("")]
    public bool DuelR { get; set; }

    [Setting]
    [DefaultValue(false)]
    [Category("Duel")]
    [DisplayName("Accept request?")]
    [Description("If true it will accept, if false it will decline")]
    public bool DuelRAccept { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Rezz")]
    [DisplayName("Handle Rezz Requests")]
    [Description("")]
    public bool RezzR { get; set; }

    [Setting]
    [DefaultValue(false)]
    [Category("Rezz")]
    [DisplayName("Accept request?")]
    [Description("If true it will accept, if false it will decline")]
    public bool RezzRAccept { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("RCheck")]
    [DisplayName("Handle Ready Checks")]
    [Description("")]
    public bool Rcheck { get; set; }

    [Setting]
    [DefaultValue(false)]
    [Category("RCheck")]
    [DisplayName("Accept check?")]
    [Description("If true it will accept, if false it will decline")]
    public bool RcheckAccept { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("LootRolls")]
    [DisplayName("Handle Loot Rolls")]
    [Description("This will select the below option when loot roll windows appear")]
    public bool LootR { get; set; }

    [Setting]
    [DefaultValue("Greed")]
    [Category("LootRolls")]
    [DisplayName("Need, Greed or Pass?")]
    [Description("")]
    [DropdownList(new string[] { "Need", "Greed", "Pass" })]
    public short LootRSetting { get; set; }

    private mRequestHandlerSettings()
    {
        PartyR = true;
        PartyRAccept = false;
        GuildR = true;
        GuildRAccept = false;
        TradeR = true;
        TradeRAccept = false;
        DuelR = true;
        DuelRAccept = false;
        RezzR = true;
        RezzRAccept = false;
        Rcheck = true;
        RcheckAccept = false;
        LootR = true;
        LootRSetting = 1;
        WaitMin = 500;
        WaitMax = 4500;
    }

    public static mRequestHandlerSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("mRequestHandler", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("mRequestHandlerSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("mRequestHandler", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =
                    Load<mRequestHandlerSettings>(AdviserFilePathAndName("mRequestHandler", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new mRequestHandlerSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("mRequestHandlerSettings > Load(): " + e);
        }
        return false;
    }
}