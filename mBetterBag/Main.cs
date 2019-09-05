using robotManager;
using robotManager.Helpful;
using robotManager.Products;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
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
        //foreach (var item in Bag.GetBagItem())
        //{
        //    Logging.Write(item.GetItemInfo.ItemType);
        //}
        //Debugger.Launch();
    }

    private void BagCheckTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        equippedBags.Clear();
        for (int i = 1; i <= 4; i++)
        {
            equippedBags.Add(GetEquippedBagInfo(i));
        }
        //foreach (var bag in equippedBags.Where(x => x.maxSlots > 0))
        //{
        //    Logging.Write($"Slot:{bag.bagSlotId}, Invid:{bag.inventoryId}, FreeSlots:{bag.freeSlots}, MaxSlots:{bag.maxSlots}");
        //}
        foreach (var bag in Bag.GetBagItem().Where(x => x.GetItemInfo.ItemType == "Container"))
        {
            Logging.Write($"BetterBag: test {bag.Name}.");
            if (equippedBags.Count(x => x.maxSlots > 0) < 4)
            {
                Lua.LuaDoString("ClearCursor()");
                Logging.Write($"BetterBag: Equipping {bag.Name}.");
                ItemsManager.EquipItemByName(bag.Name);
            }
            else if (mBetterBagSettings.CurrentSetting.ReplaceBags && equippedBags.Count(x => x.maxSlots > 0) == 4)
            {
                Logging.Write("else..");
                var smallestEquipedBag = equippedBags.Where(y => y.bagSlotId != 0).OrderBy(x => x.maxSlots).First();
                Logging.Write(smallestEquipedBag.bagSlotId.ToString() + "_" + smallestEquipedBag.inventoryId.ToString());
                if (GetInventoryBagSlotCount(bag) > smallestEquipedBag.maxSlots)
                {
                    Logging.Write($"BetterBag: {bag.Name} has more slots than {smallestEquipedBag.inventoryId}.");
                    bool moved = MoveItemsToFreeBagFrom(smallestEquipedBag.bagSlotId);
                    Logging.Write(moved.ToString());
                    if (Bag.GetBagItem().Where(x => (ulong)Bag.GetItemContainerBagIdAndSlot(x.Entry)[0] == smallestEquipedBag.bagSlotId).Count() == 0 && moved)
                    {
                        var bagidandslot = Bag.GetItemContainerBagIdAndSlot(bag.Entry);
                        Lua.LuaDoString("ClearCursor()");
                        Lua.LuaDoString($"PickupBagFromSlot({smallestEquipedBag.inventoryId})");
                        Lua.LuaDoString($"PickupContainerItem({bagidandslot[0]}, {bagidandslot[1]})");
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

    public int GetInventoryBagSlotCount(WoWItem bag)
    {
        var bagidandslot = Bag.GetItemContainerBagIdAndSlot(bag.Entry);
        var slotcount = Lua.LuaDoString<int>($@"local tip = mBetterBagTooltip or CreateFrame(""GAMETOOLTIP"", ""mBetterBagTooltip"")
                                        local L = L or tip: CreateFontString()
                                        local R = R or tip: CreateFontString()
                                        L: SetFontObject(GameFontNormal)
                                        R: SetFontObject(GameFontNormal)
                                        tip: AddFontStrings(L, R)
                                        tip: SetOwner(WorldFrame, ""ANCHOR_NONE"")
                                        local _, _, itemlink, itemid = string.find(GetContainerItemLink({bagidandslot[0]}, {bagidandslot[1]}), '|c%x+|H(item:(%d+):%d+:%d+:%d+)|h%[.-%]|h|r');
                                        tip: SetHyperlink(itemlink)
                                        for i = 1, tip:NumLines() do
                                            tmp = getglobal(""mBetterBagTooltipTextLeft""..i);
                                            if (tmp) then
                                              if string.match(tmp: GetText(), ""Slot Bag"") then
                                                return string.match(tmp: GetText(), ""(.*) Slot Bag"")
                                              end
                                            end
                                        end");
        return slotcount;
    }

    public bool MoveItemsToFreeBagFrom(ulong oldbagid)
    {
        //foreach item in the bag we're replacing, move it to another bag 
        var items = Bag.GetBagItem();
        foreach (var item in items.Where(x => (ulong)Bag.GetItemContainerBagIdAndSlot(x.Entry)[0] == oldbagid))
        {
            var bagWithFreeSlots = equippedBags.Where(x => x.freeSlots > 0 && x.bagSlotId != oldbagid).First();
            var bagidandslot = Bag.GetItemContainerBagIdAndSlot(item.Entry);
            if (bagWithFreeSlots != null)
            {
                Lua.LuaDoString("ClearCursor()");
                Logging.Write($"BetterBag: Moving {item.Name} to a better bag.");
                Lua.LuaDoString($"PickupContainerItem({bagidandslot[0]}, {bagidandslot[1]})");
                if (bagWithFreeSlots.bagSlotId == 0)
                {
                    Lua.LuaDoString("PutItemInBackpack()");
                }
                else
                {
                    Lua.LuaDoString($"PutItemInBag({bagWithFreeSlots.inventoryId})");
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
    [DisplayName("Replace existing?")]
    [Description("Will try to replace existing bags with better bags.")]
    public bool ReplaceBags { get; set; }

    //[Setting]
    //[Category("Settings")]
    //[DisplayName("Replace existing?")]
    //[Description("Will try to replace existing bags with better bags.")]
    //public bool ReplaceBags { get; set; }

    private mBetterBagSettings()
    {
        ReplaceBags = false;
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