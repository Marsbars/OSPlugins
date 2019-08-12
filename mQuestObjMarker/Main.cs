using robotManager;
using robotManager.Helpful;
using robotManager.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using wManager;
using wManager.Wow.Enums;
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : wManager.Plugin.IPlugin
{
    public List<string> ObjList = new List<string>();
    public robotManager.Helpful.Timer timer = new robotManager.Helpful.Timer(1000);
    private MD5 md5;
    private Radar3D.OnDrawHandler handler;
    private List<string> customMobList = new List<string>();
    private List<int> _hotkeys = new List<int>();

    public void Dispose()
    {
        Radar3D.OnDrawEvent -= handler;
        Radar3D.Stop();
        foreach (var hotkey in _hotkeys)
        {
            HotKeyManager.UnregisterHotKey(hotkey);
        }
    }

    public void Initialize()
    {
        mQuestObjMarkerSettings.Load();
        md5 = MD5.Create();
        //Lua.LuaDoString("SetCVar('autoInteract',0)");
        Radar3D.Pulse();
        handler = new Radar3D.OnDrawHandler(Monitor);
        Radar3D.OnDrawEvent += handler;

        _hotkeys.Add(HotKeyManager.RegisterHotKey(Keys.T, KeyModifiers.Control)); // Add Target
        _hotkeys.Add(HotKeyManager.RegisterHotKey(Keys.T, KeyModifiers.Alt)); // Remove Target
                                                                              // _hotkeys.Add(//HotKeyManager.RegisterHotKey(Keys.G, KeyModifiers.Control); // Remove Target
        _hotkeys.Add(HotKeyManager.RegisterHotKey(Keys.L, KeyModifiers.Control)); // Show Custom Target List
        _hotkeys.Add(HotKeyManager.RegisterHotKey(Keys.L, KeyModifiers.Alt)); // Clear Custom Target List
        HotKeyManager.HotKeyPressed += new EventHandler<HotKeyEventArgs>(HotKeyManager_HotKeyPressed);
    }

    private void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e)
    {
        if (e.Key == Keys.T && e.Modifiers == KeyModifiers.Control)
        {
            if (ObjectManager.Target != null)
            {
                customMobList.Add(ObjectManager.Target.Name);
                Lua.LuaDoString($"DEFAULT_CHAT_FRAME:AddMessage('Added {ObjectManager.Target.Name} to target list.')");
            }
            else
            {
                Lua.LuaDoString($"DEFAULT_CHAT_FRAME:AddMessage('No Target')");
            }
        }
        if (e.Key == Keys.T && e.Modifiers == KeyModifiers.Alt)
        {
            if (ObjectManager.Target != null)
            {
                if (customMobList.Contains(ObjectManager.Target.Name))
                {
                    customMobList.Remove(ObjectManager.Target.Name);
                    Lua.LuaDoString($"DEFAULT_CHAT_FRAME:AddMessage('Removed {ObjectManager.Target.Name} from target list.')");
                }
            }
            else
            {
                Lua.LuaDoString($"DEFAULT_CHAT_FRAME:AddMessage('No Target')");
            }
        }
        if (e.Key == Keys.L && e.Modifiers == KeyModifiers.Control)
        {
            Lua.LuaDoString($"DEFAULT_CHAT_FRAME:AddMessage('Custom Target List:')");
            foreach (var name in customMobList)
            {
                Lua.LuaDoString($"DEFAULT_CHAT_FRAME:AddMessage('{name}')");
            }
        }
        if (e.Key == Keys.L && e.Modifiers == KeyModifiers.Alt)
        {
            Lua.LuaDoString($"DEFAULT_CHAT_FRAME:AddMessage('CLEARED Custom Target List.')");
            customMobList.Clear();
        }

        //Logging.Write($"Modifier:{e.Modifiers} - Key:{e.Key}");
    }

    public void Monitor()
    {
        try
        {
            if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause)
            {
                if (timer.IsReady)
                {
                    ObjList = GetQuestObjectives();
                    timer.Reset();
                }

                List<WoWUnit> woWUnits = ObjectManager.GetObjectWoWUnit();
                List<WoWGameObject> woWGameObjects = ObjectManager.GetObjectWoWGameObject();
                List<WoWPlayer> woWPlayers = ObjectManager.GetObjectWoWPlayer();

                foreach (string obj in ObjList)
                {
                    if (mQuestObjMarkerSettings.CurrentSetting.EnableMobs)
                    {
                        List<WoWUnit> MobList = woWUnits.Where(x => obj.Contains(x.Name) && x.IsAlive && x.IsValid).ToList();
                        MobList.AddRange(woWUnits.Where(x => customMobList.Any(x.Name.Contains)));
                        foreach (WoWUnit Mob in MobList)
                        {
                            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(Mob.Name));
                            var colour = System.Drawing.Color.FromArgb(hash[0], hash[1], hash[2]);
                            if (!Mob.IsTaggedByOther)
                            {
                                Radar3D.DrawCircle(Mob.Position, 1f, colour, true, 200);
                                Radar3D.DrawLine(Mob.Position, ObjectManager.Me.Position, colour, 200);
                                //Radar3D.DrawString(Mob.Name, new Vector3(0,0,0), 10f, colour, 255, System.Drawing.FontFamily.GenericMonospace);

                            }
                        }
                    }
                    if (mQuestObjMarkerSettings.CurrentSetting.EnableGO)
                    {
                        foreach (WoWGameObject Object in woWGameObjects.Where(x => obj.Contains(x.Name) && x.IsValid))
                        {
                            Radar3D.DrawCircle(Object.Position, 1f, System.Drawing.Color.Blue, true, 150);
                            Radar3D.DrawLine(Object.Position, ObjectManager.Me.Position, System.Drawing.Color.Blue, 150);
                            //Radar3D.DrawString(Object.Name, Object.Position, 10f, System.Drawing.Color.Blue, 255, System.Drawing.FontFamily.GenericMonospace);
                        }
                    }
                }
                if (mQuestObjMarkerSettings.CurrentSetting.EnablePlayers)
                {
                    foreach (WoWPlayer Player in woWPlayers.Where(x => x.IsAlive && x.IsValid && x.PlayerFaction != ObjectManager.Me.PlayerFaction))
                    {
                        // Between Me and player
                        Radar3D.DrawCircle(Player.Position, 1f, System.Drawing.Color.Red, true);
                        Radar3D.DrawLine(Player.Position, ObjectManager.Me.Position, System.Drawing.Color.Red);
                        // Between Player and Player Target
                        if (Player.HasTarget)
                        {
                            Radar3D.DrawCircle(Player.TargetObject.Position, 0.5f, System.Drawing.Color.Orange, false, 200);
                            Radar3D.DrawLine(Player.Position, Player.TargetObject.Position, System.Drawing.Color.Orange, 200);
                        }
                        // String
                        //Radar3D.DrawString(Player.Name, Player.Position, 10f, System.Drawing.Color.Red, 255, System.Drawing.FontFamily.GenericMonospace);
                    }
                }
            }
        }
        catch
        {
            Logging.Write("qObjMarker failed to run the Monitor() function.");
        }
    }

    public List<string> GetQuestObjectives()
    {
        var objectiveList = new List<string>();
        int noOfQuests = Lua.LuaDoString<int>($"local numEntries, numQuests = GetNumQuestLogEntries(); return numEntries;");
        for (int i = 0; i <= noOfQuests; i++)
        {
            int noOfObjectives = Lua.LuaDoString<int>($"return GetNumQuestLeaderBoards({i})");
            for (int o = 1; o <= noOfObjectives; o++)
            {
                if (!Lua.LuaDoString<bool>($"local desc, type, done = GetQuestLogLeaderBoard({o}, {i}); return done;"))
                {
                    objectiveList.Add(Lua.LuaDoString<string>($"local desc, type, done = GetQuestLogLeaderBoard({o}, {i}); return desc;"));
                }
            }
        }
        return objectiveList;
    }

    public void Settings()
    {
        mQuestObjMarkerSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mQuestObjMarkerSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mQuestObjMarkerSettings.CurrentSetting.Save();
    }

    
}
[Serializable]
public class mQuestObjMarkerSettings : Settings
{

    [Setting]
    [DefaultValue(99)]
    [Category("Range")]
    [DisplayName("Search Range")]
    [Description("The search range in yards")]
    public long SearchRange { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Tracking")]
    [DisplayName("Quest Game Objects")]
    [Description("")]
    public bool EnableGO { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Tracking")]
    [DisplayName("Quest Mobs")]
    [Description("")]
    public bool EnableMobs { get; set; }

    [Setting]
    [DefaultValue(true)]
    [Category("Tracking")]
    [DisplayName("Players")]
    [Description("")]
    public bool EnablePlayers { get; set; }

    private mQuestObjMarkerSettings()
    {
        EnablePlayers = true;
        EnableMobs = true;
        EnableGO = true;
        SearchRange = 99;
    }

    public static mQuestObjMarkerSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("mQuestObjMarker", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("mQuestObjMarkerSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("mQuestObjMarker", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =
                    Load<mQuestObjMarkerSettings>(AdviserFilePathAndName("mQuestObjMarker", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new mQuestObjMarkerSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("mQuestObjMarkerSettings > Load(): " + e);
        }
        return false;
    }
}