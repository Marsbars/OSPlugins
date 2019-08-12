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
    System.Timers.Timer fashionCheck = new System.Timers.Timer() { Interval = 5000 };
    public void Dispose()
    {
        fashionCheck.Dispose();
    }

    public void Initialize()
    {
        mFashionSettings.Load();
        fashionCheck.Elapsed += FashionCheck_Elapsed;
        fashionCheck.Start();
    }

    private void FashionCheck_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        var equippedItems = EquippedItems.GetEquippedItems();
        //equippedItems.First().GetItemInfo.ItemEquipLoc
    }

    public void Settings()
    {
        mFashionSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mFashionSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mFashionSettings.CurrentSetting.Save();
    }

    [Serializable]
    public class mFashionSettings : Settings
    {

        [Setting]
        [Category("Settings")]
        [DisplayName("Equip rares")]
        [Description("When enabled the bot will equip rare quality items (blues)")]
        public bool EquipRares { get; set; }

        private mFashionSettings()
        {
            EquipRares = true;
        }

        public static mFashionSettings CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("mFashion", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("mFashionSettings > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("mFashion", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<mFashionSettings>(AdviserFilePathAndName("mFashion", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new mFashionSettings();
            }
            catch (Exception e)
            {
                Logging.WriteError("mFashionSettings > Load(): " + e);
            }
            return false;
        }
    }
}
