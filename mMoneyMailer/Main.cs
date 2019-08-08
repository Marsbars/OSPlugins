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
    private bool _isLaunched;

    public void Dispose()
    {

    }

    public void Initialize()
    {
        // Must load settings on initialize otherwise they are ignored.
        mMoneyMailerSettings.Load();
        // Check if the user has enabled mail and has a recipient set
        if (wManagerSetting.CurrentSetting.UseMail && !string.IsNullOrEmpty(wManagerSetting.CurrentSetting.MailRecipient))
        {
            // Attach our AddMonies method to the MAIL_SHOW lua event
            EventsLua.AttachEventLua((LuaEventsId)Enum.Parse(typeof(LuaEventsId), "MAIL_SHOW"), AddMonies);
            Logging.Write("[mMoneyMailer]: Attaching to LUA event.");
        }
        else
        {
            Logging.Write("[mMoneyMailer]: NOT initialized... Either mailing is disabled or recipient has not been filled out.");
        }
    }

    private void AddMonies(object context)
    {
        // Put money into a variable
        var money = ObjectManager.Me.GetMoneyCopper;
        // Check if our money is higher than the Amount to keep setting (+30 for the money it takes to send mail)
        if (money > mMoneyMailerSettings.CurrentSetting.AmountToKeep * 100 + 30)
        {
            // Put the amount to send into a variable
            var moneyToSend = money - (mMoneyMailerSettings.CurrentSetting.AmountToKeep * 100 + 30);
            // Run lua to do the sending
            Lua.LuaDoString(@"SetSendMailMoney(" + moneyToSend + "); SendMail('" + wManagerSetting.CurrentSetting.MailRecipient + "', '" + wManagerSetting.CurrentSetting.MailSubject + "', '')");
            Logging.Write("[mMoneyMailer]: Sending " + (moneyToSend / 100).ToString() + " silver.");
        }
        else
        {
            Logging.Write("[mMoneyMailer]: Money below Amount To Keep threshold, not sending");
        }
    }

    public void Settings()
    {
        mMoneyMailerSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mMoneyMailerSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mMoneyMailerSettings.CurrentSetting.Save();
    }

    [Serializable]
    public class mMoneyMailerSettings : Settings
    {

        [Setting]
        [DefaultValue(100)]
        [Category("Settings")]
        [DisplayName("Amount To Keep")]
        [Description("The amount of silver the bot keeps, rest is mailed")]
        public long AmountToKeep { get; set; }

        private mMoneyMailerSettings()
        {
            AmountToKeep = 100;
        }

        public static mMoneyMailerSettings CurrentSetting { get; set; }

        public bool Save()
        {
            try
            {
                return Save(AdviserFilePathAndName("mMoneyMailer", ObjectManager.Me.Name + "." + Usefuls.RealmName));
            }
            catch (Exception e)
            {
                Logging.WriteError("mMoneyMailerSettings > Save(): " + e);
                return false;
            }
        }

        public static bool Load()
        {
            try
            {
                if (File.Exists(AdviserFilePathAndName("mMoneyMailer", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
                {
                    CurrentSetting =
                        Load<mMoneyMailerSettings>(AdviserFilePathAndName("mMoneyMailer", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                    return true;
                }
                CurrentSetting = new mMoneyMailerSettings();
            }
            catch (Exception e)
            {
                Logging.WriteError("mMoneyMailerSettings > Load(): " + e);
            }
            return false;
        }
    }
}
