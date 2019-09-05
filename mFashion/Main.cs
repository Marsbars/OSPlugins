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
        foreach (var i in Bag.GetBagItem().Where(i => i.GetItemInfo.ItemEquipLoc.Contains("INVTYPE")))
        {

            var ii = i.GetItemInfo;
            if (ii.ItemEquipLoc == "") continue;
            if (ii.ItemEquipLoc == "INVTYPE_BAG") continue;
            SkillLine skill;
            var parsed = Enum.TryParse(ii.ItemSubType, out skill);
            if ((ii.ItemType=="Armor" || ii.ItemType=="Weapon") && parsed ? !Skill.Has(skill) : false)
            {
                Logging.Write($"Dont have skill for {i.Name}");
                continue;
            }
            if (_weaponSlots.Any(s => s == ii.ItemEquipLoc)) continue;
            var x = equippedItems.Where(g => g.GetItemInfo.ItemEquipLoc == ii.ItemEquipLoc);
            // Weapon logic
            //if( _weaponSlots.Any(s=>s == ii.ItemEquipLoc))
            //{
            //    if(ii.ItemEquipLoc == "INVTYPE_RANGED"|| ii.ItemEquipLoc == "INVTYPE_THROWN")
            //    {
            //        // ranged
            //        Logging.Write("i.Type+", "+i.GetItemInfo.ItemType+", "+i.GetItemInfo.ItemSubType);
            //    }
            //    else
            //    {
            //        // main

            //    }
            //}

            if (x.Count() >= 2)
            {
                foreach(var y in x)
                {
                    Logging.Write($"More than 1 {y.Name}");
                }                
                continue;
            }
            else if (x.Count() == 1)
            {
                if (ii.ItemLevel > x.First().GetItemInfo.ItemLevel && ii.ItemRarity >= x.First().GetItemInfo.ItemRarity && ii.ItemMinLevel <= ObjectManager.Me.Level)
                {
                    
                    
                        Logging.Write($"Equipping {x.First().Name} because it's better");
                        ItemsManager.EquipItemByName(i.Name);
                                                            
                }
            }
            else if (x.Count() == 0)
            {
                if ((ii.ItemEquipLoc == "INVTYPE_SHIELD" || ii.ItemEquipLoc == "INVTYPE_HOLDABLE") && x.Any(h => h.GetItemInfo.ItemEquipLoc == "INVTYPE_2HWEAPON"))
                {
                    continue;                    
                }
                else if ((ii.ItemEquipLoc == "INVTYPE_CHEST" || ii.ItemEquipLoc == "INVTYPE_ROBE") && x.Any(h=>h.GetItemInfo.ItemEquipLoc == "INVTYPE_CHEST" || h.GetItemInfo.ItemEquipLoc == "INVTYPE_ROBE"))
                {
                    continue;
                }
                else if (ii.ItemMinLevel <= ObjectManager.Me.Level)
                {
                    Logging.Write($"Equipping {i.Name} because we had nothing of that type");
                    ItemsManager.EquipItemByName(i.Name);
                }
            }
        }
    }

    public void Settings()
    {
        mFashionSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mFashionSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mFashionSettings.CurrentSetting.Save();
    }

    private static List<string> _weaponSlots = new List<string>
    {
        "INVTYPE_2HWEAPON",
        "INVTYPE_HOLDABLE",
        "INVTYPE_RANGED",
        "INVTYPE_THROWN",
        "INVTYPE_WEAPON",
        "INVTYPE_WEAPONMAINHAND",
        "INVTYPE_WEAPONOFFHAND",
        "INVTYPE_SHIELD"
    };
}
[Serializable]
public class mFashionSettings : Settings
{

    [Setting]
    [Category("Settings")]
    [DisplayName("Equip rares")]
    [Description("When enabled the bot will equip rare quality items (blues)")]
    public bool EquipRares { get; set; }
    [Setting]
    [Category("Settings")]
    [DisplayName("Ranged equip type")]
    [Description("Set what type of ranged weapon you want to auto equip")]
    [MarsSettingsGUI.DropdownList(new string[] { "Wands", "Throwables", "Bows", "Guns" })]
    public short RangedType { get; set; }

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