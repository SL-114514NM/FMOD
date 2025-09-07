using PlayerRoles;
using PlayerRoles.Ragdolls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.API
{
    public class Ragdoll
    {
        public static List<BasicRagdoll> BasicRagdolls = new List<BasicRagdoll>();
        public static List<Ragdoll> Ragdolls = new List<Ragdoll>();
        public static Ragdoll Get(BasicRagdoll basic)
        {
            return Ragdolls.FirstOrDefault(x => x.Base == basic);
        }
        public static Ragdoll Get(Player player)
        {
            return Ragdolls.FirstOrDefault(x => x.Owner == player);
        }
        public static Ragdoll Get(ReferenceHub referenceHub)
        {
            Player player = Player.Get(referenceHub);
            return Ragdolls.FirstOrDefault(x => x.Owner == player);
        }
        public BasicRagdoll Base { get; }
        public RagdollData RagdollData => Base.Info;
        public Vector2 Scale => RagdollData.Scale;
        public RoleTypeId RoleTypeId => RagdollData.RoleType;
        public Player Owner => Player.Get(RagdollData.OwnerHub);
        public Vector3 Position => RagdollData.StartPosition;
        public GameObject GameObject => Base.gameObject;
        public bool Frozen
        {
            get
            {
                return Base.Frozen;
            }
        }
        public Transform Transform => Base.transform;
        public Room CurrentRoom => Room.GetRoom(Position);
    }
}
