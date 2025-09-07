using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMOD.Events.EventArgs.Player;
using FMOD.Events.Interfaces;

namespace FMOD.Events.EventArgs.Handlers
{
    public class Player
    {
        public static Event<PlayerJoinArgs> PlayerJoined { get; set; } = new Event<PlayerJoinArgs>();
    }
}
