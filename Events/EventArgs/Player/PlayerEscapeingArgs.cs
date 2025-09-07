using FMOD.Events.Interfaces;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Events.EventArgs.Player
{
    public class PlayerEscapeingArgs : IFMODPlayerEvent
    {
        public PlayerEscapeingArgs(API.Player player, RoleTypeId newRole, Escape.EscapeScenarioType escapeScenario, Escape.EscapeMessage escapeMessage)
        {
            Player = player;
            IsAllowed = escapeScenario != Escape.EscapeScenarioType.None;
            NewRole = newRole;
            EscapeScenarioType = escapeScenario;
            EscapeMessage = escapeMessage;
        }
        public API.Player Player { get; set; }
        public bool IsAllowed { get; set; }
        public RoleTypeId NewRole { get; set; }
        public Escape.EscapeScenarioType EscapeScenarioType { get; set; }
        public Escape.EscapeMessage EscapeMessage { get; set; }
    }
}
