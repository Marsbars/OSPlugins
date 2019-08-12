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
    private System.Timers.Timer snipeTimer = new System.Timers.Timer();

    public void Dispose()
    {
        snipeTimer.Dispose();
    }

    public void Initialize()
    {
        mRecipeSniperSettings.Load();
        snipeTimer.Elapsed += Snipe;
        snipeTimer.Interval = mRecipeSniperSettings.CurrentSetting.BuyInterval * 1000;
        snipeTimer.Start();
    }

    public void Snipe(object sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            foreach (string item in mRecipeSniperSettings.CurrentSetting.Items)
            {
                Vendor.BuyItem(item, 1);
            }
        }
        catch
        {
            Logging.Write("it error'd.. lol");
        }
    }

    public void Settings()
    {
        mRecipeSniperSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mRecipeSniperSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mRecipeSniperSettings.CurrentSetting.Save();
    }

    
}
[Serializable]
public class mRecipeSniperSettings : Settings
{

    [Setting]
    [DefaultValue("")]
    [Category("Config")]
    [DisplayName("Items to snipe")]
    [Description("It will attempt to buy the Items every buy interval")]
    public List<string> Items { get; set; }

    [Setting]
    [DefaultValue(1000)]
    [Category("Config")]
    [DisplayName("Buy Interval")]
    [Description("Buy interval in seconds")]
    public long BuyInterval { get; set; }

    private mRecipeSniperSettings()
    {
        Items = new List<string>();
        BuyInterval = 3;
    }

    public static mRecipeSniperSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("mRecipeSniper", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("mRecipeSniperSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("mRecipeSniper", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =
                    Load<mRecipeSniperSettings>(AdviserFilePathAndName("mRecipeSniper", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new mRecipeSniperSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("mRecipeSniperSettings > Load(): " + e);
        }
        return false;
    }
}