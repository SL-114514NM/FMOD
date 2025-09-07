using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace FMOD.API
{
    public class Pickup
    {
        public static List<Pickup> List = new List<Pickup>();
        public static Pickup CreatAndSpawn(Vector3 position, ItemType itemType)
        {
            ItemBase itemBase;
            if (itemType == ItemType.None || !InventoryItemLoader.AvailableItems.TryGetValue(itemType, out itemBase))
            {
                return null;
            }
            ItemPickupBase itemPickupBase = InventorySystem.InventoryExtensions.ServerCreatePickup(itemBase, new PickupSyncInfo(itemType, itemBase.Weight), position);
            return Get(itemPickupBase);
        }
        public static Pickup Get(ItemPickupBase itemPickupBase)
        {
            return List.FirstOrDefault(x => x.itemPickupBase == itemPickupBase);
        }
        public static Pickup Get(ushort Serial)
        {
            return List.FirstOrDefault(x => x.Serial == Serial);
        }
        public ItemPickupBase itemPickupBase { get; }
        public ushort Serial => itemPickupBase.Info.Serial;
        public ItemType ItemType => itemPickupBase.Info.ItemId;
        public byte PickupSyncInfo => itemPickupBase.Info.SyncedFlags;
        public Vector3 Position
        {
            get
            {
                return itemPickupBase.Position;
            }
            set
            {
                itemPickupBase.Position = value;
            }
        }
        public GameObject GameObject => itemPickupBase.gameObject;
        public Room CurrentRoom => Room.GetRoom(Position);
        public bool IsUse => itemPickupBase.Info.InUse;
        public Quaternion Quaternion => itemPickupBase.Rotation;
        public float Weight
        {
            get
            {
                return itemPickupBase.Info.WeightKg;
            }
            set
            {
                itemPickupBase.Info.WeightKg = value;
            }
        }
        public float PickupTime
        {
            get
            {
                return 0.245f + 0.175f * this.Weight;
            }
            set
            {
                this.Weight = 0.245f - 0.175f / value;
            }
        }
        public bool IsLock
        {
            get
            {
                return itemPickupBase.Info.Locked;
            }
            set
            {
                itemPickupBase.Info.Locked = value;
            }
        }
        public void Destroyed()
        {
            itemPickupBase.DestroySelf();
        }
        public PickupStandardPhysics PickupStandardPhysics
        {
            get
            {
                return this.itemPickupBase.PhysicsModule as PickupStandardPhysics;
            }
        }
        public PickupPhysicsModule PhysicsModule
        {
            get
            {
                return this.itemPickupBase.PhysicsModule;
            }
        }
        public Rigidbody Rigidbody
        {
            get
            {
                PickupStandardPhysics pickupStandardPhysics = this.PickupStandardPhysics;
                if (pickupStandardPhysics == null)
                {
                    return null;
                }
                return pickupStandardPhysics.Rb;
            }
        }
        public Transform Transform { get; }
        public void Spawn()
        {
            NetworkServer.Spawn(this.GameObject);
        }
    }
}
