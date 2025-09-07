using InventorySystem.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.Extensions
{
    public static class ItemTypeExtensions
    {
        public static ItemBase GetItemBase(this ItemType itemType)
        {
            var allItems = GameObject.FindFirstObjectByType<ItemBase>();

            if (allItems.ItemTypeId == itemType)
            {
                    return allItems;
            }
            return null;
        }
        public static bool IsAmmo(this ItemType itemType)
        {
            if (itemType.GetItemBase().Category == ItemCategory.Ammo)
            {
                return true;
            }
            return false;
        }
        public static bool IsKeycard(this ItemType itemType)
        {
            return itemType.GetItemBase().Category == ItemCategory.Keycard;
        }
        public static bool IsMedical(this  ItemType itemType)
        {
            return itemType.GetItemBase().Category == ItemCategory.Medical;
        }
        public static bool IsSCPItem(this ItemType itemType)
        {
            return itemType.GetItemBase().Category == ItemCategory.SCPItem;
        }

    }
}
