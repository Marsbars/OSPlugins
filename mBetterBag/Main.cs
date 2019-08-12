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
    public System.Timers.Timer bagCheckTimer = new System.Timers.Timer();
    public List<EquippedBagInfo> equippedBags = new List<EquippedBagInfo>();

    public void Dispose()
    {
        bagCheckTimer.Elapsed -= BagCheckTimer_Elapsed;
        bagCheckTimer.Dispose();
    }

    public void Initialize()
    {
        // PLUGIN NOT IN A WORKING STATE.
        // PLUGIN NOT IN A WORKING STATE.
        // PLUGIN NOT IN A WORKING STATE.
        mBetterBagSettings.Load();
        bagCheckTimer.Elapsed += BagCheckTimer_Elapsed;
        bagCheckTimer.Interval = 5000;
        bagCheckTimer.Start();
        foreach (var item in Bag.GetBagItem())
        {
            Logging.Write(item.GetItemInfo.ItemType);
        }
    }

    private void BagCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        equippedBags.Clear();
        for (int i = 0; i <= 4; i++)
        {
            equippedBags.Add(GetEquippedBagInfo(i));
        }
        foreach (var bag in equippedBags.Where(x => x.maxSlots > 0))
        {
            Logging.Write($"Slot:{bag.bagSlotId}, Invid:{bag.inventoryId}, FreeSlots:{bag.freeSlots}, MaxSlots:{bag.maxSlots}");
        }
        foreach (var bag in Bag.GetBagItem().Where(x => x.GetItemInfo.ItemType == "Container"))
        {
            if (equippedBags.Count(x => x.maxSlots > 0) < 5)
            {
                Lua.LuaDoString("ClearCursor()");
                //ItemsManager.UseItem((uint)bag.Entry);
                ItemsManager.EquipItemByName(bag.Name);
            }
            else
            {
                var smallestEquipedBag = equippedBags.Where(y => y.bagSlotId != 0).OrderBy(x => x.maxSlots).First();
                if (GetContainerNumSlots(bag.Entry) > smallestEquipedBag.maxSlots)
                {
                    bool moved = MoveItemsToFreeBagFrom(smallestEquipedBag.bagSlotId);
                    if (Bag.GetBagItem().Where(x => x.ContainedIn == smallestEquipedBag.bagSlotId).Count() == 0 && moved)
                    {
                        Lua.LuaDoString("ClearCursor()");
                        Lua.LuaDoString($"PickupBagFromSlot({smallestEquipedBag.inventoryId})");
                        Bag.PickupContainerItem(bag.Entry);
                    }
                }
            }
        }
    }

    public int GetContainerNumSlots(int bagid)
    {
        return Lua.LuaDoString<int>($"return GetContainerNumSlots({bagid})");
    }

    public int ContainerIDToInventoryID(int bagid)
    {
        return Lua.LuaDoString<int>($"return ContainerIDToInventoryID({bagid})");
    }

    public bool MoveItemsToFreeBagFrom(ulong oldbagid)
    {
        //foreach item in the bag we're replacing, move it to another bag 
        foreach (WoWItem item in Bag.GetBagItem().Where(x => x.ContainedIn == oldbagid))
        {

            var bagWithFreeSlots = equippedBags.Where(x => x.freeSlots > 0 && x.bagSlotId != oldbagid);
            if (bagWithFreeSlots.Count() > 0)
            {
                Lua.LuaDoString("ClearCursor()");
                Logging.Write($"BetterBag: Moving {item.Name} to a better bag.");
                Bag.PickupContainerItem(Bag.GetItemContainerBagIdAndSlot(item.Entry));
                if (bagWithFreeSlots.Single().bagSlotId == 0)
                {
                    Lua.LuaDoString("PutItemInBackpack()");
                }
                else
                {
                    Lua.LuaDoString($"PutItemInBag({bagWithFreeSlots.Single().inventoryId})");
                }
            }
            else
            {
                return false;
            }
        }
        return true;
    }

    public EquippedBagInfo GetEquippedBagInfo(int bagid)
    {
        var b = new EquippedBagInfo();
        b.bagSlotId = (ulong)bagid;
        b.inventoryId = Lua.LuaDoString<int>($"return ContainerIDToInventoryID({bagid})");
        b.freeSlots = Bag.GetContainerNumFreeSlotsByBagID(bagid);
        b.maxSlots = Lua.LuaDoString<int>($"return GetContainerNumSlots({bagid})");
        return b;
    }

    public class EquippedBagInfo
    {
        public ulong bagSlotId;
        public int inventoryId;
        public int maxSlots;
        public int freeSlots;
    }

    public void Settings()
    {
        mBetterBagSettings.Load();
        var settingWindow = new MarsSettingsGUI.SettingsWindow(mBetterBagSettings.CurrentSetting, ObjectManager.Me.WowClass.ToString());
        settingWindow.ShowDialog();
        mBetterBagSettings.CurrentSetting.Save();
    }

    
}
[Serializable]
public class mBetterBagSettings : Settings
{

    [Setting]
    [Category("Settings")]
    [DisplayName("Number")]
    [Description("Description")]
    public bool ReplaceBags { get; set; }

    private mBetterBagSettings()
    {
        ReplaceBags = true;
    }

    public static mBetterBagSettings CurrentSetting { get; set; }

    public bool Save()
    {
        try
        {
            return Save(AdviserFilePathAndName("mBetterBag", ObjectManager.Me.Name + "." + Usefuls.RealmName));
        }
        catch (Exception e)
        {
            Logging.WriteError("mBetterBagSettings > Save(): " + e);
            return false;
        }
    }

    public static bool Load()
    {
        try
        {
            if (File.Exists(AdviserFilePathAndName("mBetterBag", ObjectManager.Me.Name + "." + Usefuls.RealmName)))
            {
                CurrentSetting =
                    Load<mBetterBagSettings>(AdviserFilePathAndName("mBetterBag", ObjectManager.Me.Name + "." + Usefuls.RealmName));
                return true;
            }
            CurrentSetting = new mBetterBagSettings();
        }
        catch (Exception e)
        {
            Logging.WriteError("mBetterBagSettings > Load(): " + e);
        }
        return false;
    }
}