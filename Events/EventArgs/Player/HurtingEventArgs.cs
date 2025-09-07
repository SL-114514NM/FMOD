using FMOD.API;
using FMOD.API.DamageHandles;
using FMOD.Events.Interfaces;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Events.EventArgs.Player
{
    public class HurtingEventArgs: IFMODPlayerEvent, IAttackerEvent
    {
        public HurtingEventArgs(API.Player Target, DamageBase damageHandlerBase)
        {
            Player = Target;
            DamageHandler = damageHandlerBase;
            Attacker = damageHandlerBase.Attacker;
        }
        public API.Player Player { get; set; }
        public bool IsAllowed {  get; set; }

        public API.Player Attacker {  get;}

        public DamageBase DamageHandler {  get; }
        public float Amount
        {
            get => DamageHandler.Damage;
            set => DamageHandler.Damage = value;
        }
    }
}
