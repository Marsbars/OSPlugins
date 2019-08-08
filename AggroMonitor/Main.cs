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
using wManager.Wow.Helpers;
using wManager.Wow.ObjectManager;

public class Main : wManager.Plugin.IPlugin
{
    private bool _isLaunched;

    public void Dispose()
    {
        _isLaunched = false;
        Radar3D.Stop();
    }

    public void Initialize()
    {
        mAggroMonitorSettings.Load();
        _isLaunched = true;
        Radar3D.Pulse();
        Radar3D.OnDrawEvent += new Radar3D.OnDrawHandler(Monitor);
    }

    public void Monitor()
    {
        try
        {
            if (Conditions.InGameAndConnectedAndAliveAndProductStartedNotInPause && Fight.InFight && _isLaunched)
            {
                foreach (WoWUnit Mob in ObjectManager.GetWoWUnitAttackables().Where(x => x.GetDistance2D < mAggroMonitorSettings.CurrentSetting.SearchRange && x.TargetObject.Name != mAggroMonitorSettings.CurrentSetting.Tank1 && x.TargetObject.Name != mAggroMonitorSettings.CurrentSetting.Tank2 && x.TargetObject.Name != mAggroMonitorSettings.CurrentSetting.Tank3 && x.TargetObject.Name != ObjectManager.Me.Name && ObjectManager.Target.TargetObject != null))
                {
                    Radar3D.DrawCircle(ObjectManager.Target.Position, 1f, System.Drawing.Color.Red, true);
                    Radar3D.DrawLine(Mob.Position, Mob.TargetObject.Position, System.Drawing.Color.Red);
                    Radar3D.DrawCircle(Mob.TargetObject.Position, 0.5f, System.Drawing.Color.LightBlue, false);
                }
            }
        }
        catch
        {
            Logging.Write("it error'd.. lol");
        }
    }

    public void Settings()
    {
        mAggroMonitorSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mAggroMonitorSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mAggroMonitorSettings.CurrentSetting.Save();
    }

    [Serializable]
    public class mAggroMonitorSettings : Settings
    {
        [Setting]
        [DefaultValue("")]
        [Category("Tanks")]
        [DisplayName("Tank #1")]
        [Description("Name a tank you wish to ignore")]
        public string Tank1 { get; set; }

        [Setting]
        [DefaultValue("")]
        [Category("Tanks")]
        [DisplayName("Tank #2")]
        [Description("Name a tank you wish to ignore")]
        public string Tank2 { get; set; }

        [Setting]
        [DefaultValue("")]
        [Category("Tanks")]
        [DisplayName("Tank #3")]
        [Description("Name a tank you wish to ignore")]
        public string Tank3 { get; set; }

        [Setting]
        [DefaultValue(20)]
        [Category("Range")]
        [DisplayName("Search Range")]
        [Description("The search range in yards")]
        public int SearchRange { get; set; }

        private mAggroMonitorSettings()
        {
            Tank1 = "";
            Tank2 = "";
            Tank3 = "";
            SearchRange = 20;

            ConfigWinForm(new System.Drawing.Point(300, 400), "mAggroMonitor " + Translate.Get("Settings"));
        }

        public static mAggroMonitorSettings CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("mAggroMonitor", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("mAggroMonitorSettings > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("mAggroMonitor", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<mAggroMonitorSettings>(AdviserFilePathAndName("mAggroMonitor", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new mAggroMonitorSettings();
            }
            catch (Exception e)
            {
                Logging.WriteError("mAggroMonitorSettings > Load(): " + e);
            }
            return false;
        }
    }
}
