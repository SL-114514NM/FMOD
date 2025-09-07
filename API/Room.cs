using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.API
{
    public class Room
    {
        public static List<RoomIdentifier> IdentifierList = new List<RoomIdentifier>();
        public static List<Room> List = new List<Room>();
        public RoomIdentifier RoomIdentifier { get; }
        public RoomName Name { get; }
        public Vector3 Position => RoomIdentifier.transform.position;
        public RoomShape RoomShape { get; }
        public RoomLight Light { get; }
        public GameObject GameObject => RoomIdentifier.gameObject;
        public Transform Transform => RoomIdentifier.transform;
        public FacilityZone Zone => RoomIdentifier.Zone;
        public Quaternion Quaternion => GameObject.transform.rotation;
        public Vector3 Scale => GameObject.transform.localScale;
        public RoomLightController RoomLightController => RoomIdentifier.LightControllers.FirstOrDefault();
        public static Room GetRoom(RoomName roomName)
        {
            return List.First(x => x.Name == roomName);
        }
        public static Vector3 GetPositionToWord(RoomName roomName, Vector3 position)
        {
            var roomInfo = GetRoom(roomName);
            Vector3 rotatedPosition = roomInfo.GameObject.transform.rotation * position;
            Vector3 worldPosition = roomInfo.GameObject.transform.position + rotatedPosition;
            return worldPosition;
        }
        public static void TryGetWordPosition(RoomName roomName, Vector3 pos, out Vector3 Wordpos)
        {
            Wordpos = GetPositionToWord(roomName, pos);
        }
        public static Room GetRoom(Vector3 Position)
        {
            return List.First(x => x.Position == Position);
        }
        public static Room GetRoom(RoomShape roomShape)
        {
            return List.First(x => x.RoomShape == roomShape);
        }
        public static RoomIdentifier GetIdentifier(RoomName roomName)
        {
            return IdentifierList.FirstOrDefault(x => x.Name == roomName);
        }
        public static RoomIdentifier GetIdentifier(Vector3 vector3)
        {
            return IdentifierList.FirstOrDefault(x => x.transform.position == vector3);
        }
        public static RoomIdentifier GetIdentifier(RoomShape roomShape)
        {
            return IdentifierList.FirstOrDefault(x => x.Shape == roomShape);
        }
        public static RoomIdentifier TryGetIdentifier(RoomName roomName, out RoomIdentifier result)
        {
            return result = GetIdentifier(roomName);
        }
        public static RoomIdentifier TryGetIdentifier(RoomShape roomShape, out RoomIdentifier result)
        {
            return result = GetIdentifier(roomShape);
        }
        public static RoomIdentifier TryGetIdentifier(Vector3 vector3, out RoomIdentifier result)
        {
            return result = GetIdentifier(vector3);
        }
        public void ChangColor(UnityEngine.Color color)
        {
            RoomLightController.NetworkOverrideColor = color;
        }
        public bool LightIsOffOrOn
        {
            get
            {
               return RoomLightController.LightsEnabled;
            }
            set
            {
                RoomLightController.LightsEnabled = value;
            }
        }
    }
}
