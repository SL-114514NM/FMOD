using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.API.Roles
{
    public class Role
    {
        public RoleTypeId RoleTypeId => Base.RoleTypeId;
        public string RoleName => Base.RoleName;
        public Color RoleColor => Base.RoleColor;
        public PlayerRoleBase Base { get; }
        public Team Team => Base.Team;
        public Player Owner { get; }
        public RoleChangeReason SpawnReason => Base.ServerSpawnReason;
        public RoleSpawnFlags SpawnFlags => Base.ServerSpawnFlags;

        public Role(Player owner, PlayerRoleBase roleBase)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Base = roleBase ?? throw new ArgumentNullException(nameof(roleBase));
        }

        public void SetRole(RoleTypeId newRole, RoleChangeReason reason)
        {
            if (Owner?.ReferenceHub?.roleManager == null)
                return;

            Owner.ReferenceHub.roleManager.ServerSetRole(newRole, reason);
        }

        public void SetRole(RoleTypeId newRole)
        {
            SetRole(newRole, RoleChangeReason.None);
        }

        public bool TryGetRole<T>(out T role) where T : PlayerRoleBase
        {
            role = Base as T;
            return role != null;
        }

        public bool IsRole<T>() where T : PlayerRoleBase
        {
            return Base is T;
        }

        public T GetRole<T>() where T : PlayerRoleBase
        {
            if (Base is T role)
                return role;

            throw new InvalidOperationException($"Current role is not of type {typeof(T).Name}");
        }

        public bool TryGetRole<T>() where T : PlayerRoleBase
        {
            return Base is T;
        }
    }
}
