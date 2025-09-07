using FMOD.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Events.EventArgs.Player
{
    public class PlayerLeftArgs : Interfaces.IFMODPlayerEvent
    {
        public PlayerLeftArgs(API.Player player)
        {
            Player = player;
            IsAllowed = true;
        }
        public API.Player Player {  get; set; }
        public bool IsAllowed {  get; set; }
    }
}
