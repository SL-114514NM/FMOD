using Footprinting;
using PlayerRoles.PlayableScps.Scp3114;
using PlayerStatsSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API.DamageHandles
{
    public class DamageBase : DamageHandlerBase
    {
        public override string RagdollInspectText { get; }

        public override string DeathScreenText { get; }

        public override string ServerLogsText { get; }

        public override string ServerMetricsText { get; }

        public override CassieAnnouncement CassieDeathAnnouncement { get; }

        public override HandlerOutput ApplyDamage(ReferenceHub ply)
        {
            throw new NotImplementedException();
        }
        public DamageHandlerBase Base { get; set; }
        public StandardDamageHandler StandardDamageHandler { get; set; }
        public CustomReasonDamageHandler CustomReasonDamageHandler { get; set; }
        public WarheadDamageHandler WarheadDamageHandler { get; set; }
        public ExplosionDamageHandler ExplosionDamageHandler { get; set; }
        public FirearmDamageHandler FirearmDamageHandler { get; set; }
        public AttackerDamageHandler AttackerDamageHandler { get; set; }
        public DisruptorDamageHandler DisruptorDamageHandler { get; set; }
        public CustomReasonFirearmDamageHandler CustomReasonFirearmDamageHandler { get; set; }
        public ScpDamageHandler ScpDamageHandler { get; set; }
        public Scp096DamageHandler Scp096DamageHandler { get; set; }
        public Scp049DamageHandler Scp049DamageHandler { get; set; }
        public Scp3114DamageHandler Scp3114DamageHandler { get; set; }
        public Scp018DamageHandler Scp018DamageHandler { get; set; }
        public PlayerStats PlayerStats { get; set; }
        public MicroHidDamageHandler MicroHidDamageHandler { get; set; }
        public JailbirdDamageHandler JailbirdDamageHandler { get; set; }
        public UniversalDamageHandler UniversalDamageHandler { get; set; }
        public DeathTranslation DeathTranslations { get; set; }

        public T As<T>() where T : DamageHandlerBase
        {
            return this.Base as T;
        }
        public T BaseAs<T>() where T : DamageHandlerBase
        {
            return this as T;
        }
        public bool BaseIs<T>(out T param) where T : DamageHandlerBase
        {
            param = default(T);
            T t = this as T;
            if (t == null)
            {
                return false;
            }
            param = t;
            return true;
        }

        public Player Target { get; set; }
        public Player Attacker { get; set; }
        public float AbsorbedAhpDamage
        {
            get
            {
                StandardDamageHandler standardDamageHandler;
                if (!BaseIs<StandardDamageHandler>(out standardDamageHandler))
                {
                    return 0f;
                }
                return standardDamageHandler.AbsorbedAhpDamage;
            }
        }
        public Footprint TargetFootprint { get; set; }
        public virtual float Damage
        {
            get
            {
                StandardDamageHandler standardDamageHandler;
                if (!BaseIs<StandardDamageHandler>(out standardDamageHandler))
                {
                    return 0f;
                }
                return standardDamageHandler.Damage;
            }
            set
            {
                StandardDamageHandler standardDamageHandler;
                if (BaseIs<StandardDamageHandler>(out standardDamageHandler))
                {
                    standardDamageHandler.Damage = value;
                }
            }
        }

    }
}
