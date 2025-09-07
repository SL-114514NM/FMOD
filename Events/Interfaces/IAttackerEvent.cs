using FMOD.API.DamageHandles;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Events.Interfaces
{
    public interface IAttackerEvent: IPlayerEvent
    {
        API.Player Attacker { get; }
        DamageBase DamageHandler { get; }
    }
}
