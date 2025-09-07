using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API
{
    public class Map
    {
        public static List<Item> Items = new List<Item>();
        public static List<Room> Rooms = new List<Room>();
        public static List<Pickup> Pickups = new List<Pickup>();
        public static void ChangRoomColor(Room room, UnityEngine.Color color)
        {
            room.RoomLightController.NetworkOverrideColor = color;
        }
        public static void SendBroadcast(string msg, ushort time)
        {
            Broadcast.Singleton.RpcAddElement(msg, time, Broadcast.BroadcastFlags.Normal);
        }
    }
}
