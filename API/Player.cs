using CentralAuth;
using CommandSystem;
using CommandSystem.Commands.RemoteAdmin.Dms;
using CustomPlayerEffects;
using FMOD.API.Roles;
using FMOD.API.ServerSpecific;
using FMOD.API.SSAudio;
using FMOD.API.SSHint;
using FMOD.Extensions;
using Footprinting;
using Hints;
using InventorySystem;
using InventorySystem.Disarming;
using InventorySystem.Items;
using InventorySystem.Items.Usables;
using InventorySystem.Items.Usables.Scp330;
using LabApi.Features.Wrappers;
using MapGeneration;
using MEC;
using Mirror;
using Org.BouncyCastle.Cms;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.Thirdperson.Subcontrollers;
using PlayerRoles.Voice;
using PlayerStatsSystem;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VoiceChat;
using static RoundSummary;

namespace FMOD.API
{
    public class Player
    {
        public static List<ReferenceHub> ReferenceHubs = new List<ReferenceHub>();
        public static List<Player> List = new List<Player>();
        public static List<Player> RemoteAdmins => Player.List.Where(x => x.RemoteAdminAccess).ToList();
        public static List<Player> DummyCount => Player.List.Where(x => x.IsDummy).ToList();
        public static Player Get(RoleTypeId roleTypeId)
        {
            return List.FirstOrDefault(x => x.ReferenceHub.roleManager.CurrentRole.RoleTypeId == roleTypeId);
        }
        public static Player Get(int Id)
        {
            return List.FirstOrDefault(x => x.Id == Id);
        }
        public static Player Get(string UserID)
        {
            return List.FirstOrDefault(x => x.UserId == UserID);
        }
        public static Player Get(GameObject gameObject)
        {
            return List.FirstOrDefault(x => x.GameObject == gameObject);
        }
        public static Player Get(ICommandSender commandSender)
        {
            CommandSender Sender = commandSender as CommandSender;
            return Get(Sender.SenderId);
        }
        public static Player Get(Footprint footprint)
        {
            return Get(footprint.Hub);
        }

        public static Player Get(ReferenceHub referenceHub)
        {
            return List.FirstOrDefault(x => x.ReferenceHub == referenceHub);
        }
        public ReferenceHub ReferenceHub;
        public Transform Transform => ReferenceHub.transform;
        public Role Role { get; }
        public int Id => ReferenceHub.PlayerId;
        public string UserId => ReferenceHub.authManager.UserId;
        public GameObject GameObject => ReferenceHub.gameObject;
        public virtual Vector3 Position
        {
            get
            {
                return this.Transform.position;
            }
            set
            {
                this.ReferenceHub.TryOverridePosition(value);
            }
        }
        public bool IsDummy
        {
            get
            {
                return this.ReferenceHub.authManager.InstanceMode == ClientInstanceMode.Dummy;
            }
        }

        public Quaternion Quaternion => GameObject.transform.rotation;
        public Room CurrentRoom => Room.GetRoom(Position);
        public Inventory Inventory => ReferenceHub.inventory;
        public void Hurt(DamageHandlerBase damageHandlerBase)
        {
            this.ReferenceHub.playerStats.DealDamage(damageHandlerBase);
        }
        public void Hurt(float damage)
        {
            Hurt(new UniversalDamageHandler(damage, DeathTranslations.Unknown));
        }
        public void Kill()
        {
            this.Hurt(new UniversalDamageHandler(-1f, DeathTranslations.Unknown, null));
        }
        public void Kill(string msg)
        {
            this.Hurt(new CustomReasonDamageHandler(msg));
        }
        public float Health
        {
            get
            {
               return ReferenceHub.playerStats.GetModule<HealthStat>().CurValue;
            }
            set
            {
                ReferenceHub.playerStats.GetModule<HealthStat>().CurValue = value;
            }
        }
        public float MaxHealth
        {
            get
            {
                return this.ReferenceHub.playerStats.GetModule<HealthStat>().MaxValue;
            }
            set
            {
                this.ReferenceHub.playerStats.GetModule<HealthStat>().MaxValue = value;
            }
        }


        public void AddItem(ItemType itemType)
        {
            ReferenceHub.inventory.ServerAddItem(itemType, InventorySystem.Items.ItemAddReason.AdminCommand);
        }
        public void AddItem(ItemType itemType, InventorySystem.Items.ItemAddReason reason)
        {
            ReferenceHub.inventory.ServerAddItem(itemType, reason);
        }
        public void DisableAllEffects()
        {
            StatusEffectBase[] allEffects = this.ReferenceHub.playerEffectsController.AllEffects;
            for (int i = 0; i < allEffects.Length; i++)
            {
                allEffects[i].IsEnabled = false;
            }
        }
        public void RemoveItem(ItemBase item)
        {
            this.Inventory.ServerRemoveItem(item.ItemSerial, null);
        } 
        public void RemoveItem(ItemType itemType)
        {
            RemoveItem(itemType.GetItemBase());
        }
        public string Nickname
        {
            get
            {
                return this.ReferenceHub.nicknameSync.MyNick;
            }
        }
        public Team Team => ReferenceHub.roleManager.CurrentRole.Team;
        public Vector3 Scale => GameObject.transform.localScale;
        public VoiceModuleBase VoiceModule
        {
            get
            {
                IVoiceRole voiceRole = this.Role.Base as IVoiceRole;
                if (voiceRole == null)
                {
                    return null;
                }
                return voiceRole.VoiceModule;
            }
        }
        public VoiceChatChannel VoiceChannel
        {
            get
            {
                VoiceModuleBase voiceModule = this.VoiceModule;
                if (voiceModule == null)
                {
                    return VoiceChatChannel.None;
                }
                return voiceModule.CurrentChannel;
            }
            set
            {
                Interface.IVoiceRole voiceRole = this.Role.Base as Interface.IVoiceRole;
                voiceRole.VoiceChatChannel = value;
            }
        }
        public uint NetworkId => ReferenceHub.characterClassManager.netId;
        public FacilityZone Zone => CurrentRoom.Zone;
        public string DisplayName => ReferenceHub.nicknameSync.DisplayName;
        public string IP => ReferenceHub.characterClassManager.connectionToClient.address;
        public IReadOnlyCollection<Item> Items { get; }
        public void Kick(string MSG)
        {
            LabApi.Features.Wrappers.Server.KickPlayer(LabApi.Features.Wrappers.Player.Get(UserId), MSG);
        }
        public void Kick()
        {
            Kick(null);
        }
        public void Ban(string msg, long time)
        {
            LabApi.Features.Wrappers.Server.BanPlayer(LabApi.Features.Wrappers.Player.Get(UserId), msg, time);
        }
        public bool RemoteAdminAccess => ReferenceHub.serverRoles.RemoteAdmin;
        public PlayerPermissions RemoteAdminPermissions => (PlayerPermissions)this.ReferenceHub.serverRoles.Permissions;
        public PlayerCommandSender Sender => GetSender();
        public PlayerCommandSender GetSender()
        {
            ReferenceHub.queryProcessor.TryGetSender(out var sender);
            return sender;
        }
        public float Stamina => ReferenceHub.playerStats.GetModule<StaminaStat>().CurValue;
        public float MaxStamina => ReferenceHub.playerStats.GetModule<StaminaStat>().MaxValue;
        public IEnumerable<StatusEffectBase> ActiveEffects
        {
            get
            {
                return from effect in this.ReferenceHub.playerEffectsController.AllEffects
                       where effect.Intensity > 0
                       select effect;
            }
        }
        public bool EnableEffect(StatusEffectBase statusEffect, byte intensity, float duration = 0f, bool addDurationIfActive = false)
        {
            if (statusEffect == null)
            {
                return false;
            }
            statusEffect.ServerSetState(intensity, duration, addDurationIfActive);
            return statusEffect != null && statusEffect.IsEnabled;
        }
        public bool EnableEffect(StatusEffectBase statusEffect, float duration = 0f, bool addDurationIfActive = false)
        {
            return this.EnableEffect(statusEffect, 1, duration, addDurationIfActive);
        }
        public StatusEffectBase EnableEffect(string effectName, float duration = 0f, bool addDurationIfActive = false)
        {
            return this.EnableEffect(effectName, 1, duration, addDurationIfActive);
        }
        public StatusEffectBase EnableEffect(string effectName, byte intensity, float duration = 0f, bool addDurationIfActive = false)
        {
            return this.ReferenceHub.playerEffectsController.ChangeState(effectName, intensity, duration, addDurationIfActive);
        }
        public bool IsHasItem(ItemType itemType) => Items.Any(x => x.Type == itemType);
        public bool IsHasItem(Item item) => Items.Contains(item);
        public void Heal(float amount, bool overrideMaxHealth = false)
        {
            if (!overrideMaxHealth)
            {
                this.ReferenceHub.playerStats.GetModule<HealthStat>().ServerHeal(amount);
                return;
            }
            Health += amount;
        }
        public Vector3 Velocity
        {
            get
            {
                return this.ReferenceHub.GetVelocity();
            }
        }
        public bool IsCuffed
        {
            get
            {
                return this.Inventory.IsDisarmed();
            }
        }
        public bool IsJumping
        {
            get
            {
                IFpcRole fpcRole = this.Role.Base as IFpcRole;
                return fpcRole != null && fpcRole.FpcModule.Motor.JumpController.IsJumping;
            }
        }
        public bool IsHost
        {
            get
            {
                return this.ReferenceHub.isLocalPlayer;
            }
        }

        public bool IsAlive
        {
            get
            {
                return !this.IsDead;
            }
        }

        public bool IsDead
        {
            get
            {
                return Role.RoleTypeId == RoleTypeId.Spectator;
            }
        }
        public bool IsCH => Team == Team.ChaosInsurgency;
        public bool IsMTF => Team == Team.FoundationForces;
        public bool IsSCP => Team == Team.SCPs;
        public bool IsClassD => Role.RoleTypeId == RoleTypeId.ClassD;
        public bool IsScientist => Role.RoleTypeId == RoleTypeId.Scientist;
        public bool IsHuman
        {
            get
            {
                bool B = Role.RoleTypeId != RoleTypeId.Spectator && Team != Team.SCPs;
                return B;
            }
        }
        public bool IsBypassModeEnabled
        {
            get
            {
                return this.ReferenceHub.serverRoles.BypassMode;
            }
            set
            {
                this.ReferenceHub.serverRoles.BypassMode = value;
            }
        }
        public EmotionPresetType Emotion
        {
            get
            {
                return EmotionSync.GetEmotionPreset(this.ReferenceHub);
            }
            set
            {
                this.ReferenceHub.ServerSetEmotionPreset(value);
            }
        }
        public bool IsMuted
        {
            get
            {
                return VoiceChatMutes.QueryLocalMute(this.UserId, false);
            }
            set
            {
                if (value)
                {
                    this.VoiceChatMuteFlags |= VcMuteFlags.LocalRegular;
                    return;
                }
                this.VoiceChatMuteFlags &= ~VcMuteFlags.LocalRegular;
            }
        }

        public bool IsGlobalMuted
        {
            get
            {
                return VoiceChatMutes.IsMuted(ReferenceHub) && this.VoiceChatMuteFlags.HasFlag(VcMuteFlags.GlobalRegular);
            }
            set
            {
                if (value)
                {
                    this.VoiceChatMuteFlags |= VcMuteFlags.GlobalRegular;
                    return;
                }
                this.VoiceChatMuteFlags &= ~VcMuteFlags.GlobalRegular;
            }
        }

        public bool IsIntercomMuted
        {
            get
            {
                return VoiceChatMutes.QueryLocalMute(this.UserId, true);
            }
            set
            {
                if (value)
                {
                    this.VoiceChatMuteFlags |= VcMuteFlags.LocalIntercom;
                    return;
                }
                this.VoiceChatMuteFlags &= ~VcMuteFlags.LocalIntercom;
            }
        }
        public VcMuteFlags VoiceChatMuteFlags
        {
            get
            {
                return VoiceChatMutes.GetFlags(this.ReferenceHub);
            }
            set
            {
                VoiceChatMutes.SetFlags(this.ReferenceHub, value);
            }
        }
        public string AuthenticationToken
        {
            get
            {
                return this.ReferenceHub.authManager.GetAuthToken();
            }
        }
        public bool IsNPC
        {
            get
            {
                return this.ReferenceHub.IsDummy;
            }
        }
        public bool HasCustomName
        {
            get
            {
                return this.ReferenceHub.nicknameSync.HasCustomName;
            }
        }
        public string CustomName
        {
            get
            {
                return this.ReferenceHub.nicknameSync.Network_displayName ?? this.Nickname;
            }
            set
            {
                this.ReferenceHub.nicknameSync.Network_displayName = value;
            }
        }
        public PlayerInfoArea InfoArea
        {
            get
            {
                return this.ReferenceHub.nicknameSync.Network_playerInfoToShow;
            }
            set
            {
                this.ReferenceHub.nicknameSync.Network_playerInfoToShow = value;
            }
        }
        public string CustomInfo
        {
            get
            {
                return this.ReferenceHub.nicknameSync.Network_customPlayerInfoString;
            }
            set
            {
                string str;
                if (!NicknameSync.ValidateCustomInfo(value, out str))
                {
                    Log.Error("Could not set CustomInfo for " + this.Nickname + ". Reason: " + str);
                }
                this.InfoArea = (string.IsNullOrEmpty(value) ? (this.InfoArea & ~PlayerInfoArea.CustomInfo) : (this.InfoArea |= PlayerInfoArea.CustomInfo));
                this.ReferenceHub.nicknameSync.Network_customPlayerInfoString = value;
            }
        }
        public bool DoNotTrack
        {
            get
            {
                return this.ReferenceHub.authManager.DoNotTrack;
            }
        }

        public bool IsConnected
        {
            get
            {
                return this.GameObject != null;
            }
        }
        public bool IsWhitelisted
        {
            get
            {
                return WhiteList.IsWhitelisted(this.UserId);
            }
        }
        public bool IsOverwatchEnabled
        {
            get
            {
                return this.ReferenceHub.serverRoles.IsInOverwatch;
            }
            set
            {
                this.ReferenceHub.serverRoles.IsInOverwatch = value;
            }
        }

        public bool IsNoclipPermitted
        {
            get
            {
                return FpcNoclip.IsPermitted(this.ReferenceHub);
            }
            set
            {
                if (value)
                {
                    FpcNoclip.PermitPlayer(this.ReferenceHub);
                    return;
                }
                FpcNoclip.UnpermitPlayer(this.ReferenceHub);
            }
        }
        public bool IsSpeaking
        {
            get
            {
                IVoiceRole voiceRole = Role.Base as IVoiceRole;
                return voiceRole != null && voiceRole.VoiceModule.ServerIsSending;
            }
        }
        public void SendHint(string text, float duration = 3f)
        {
            this.SendHint(text, new HintParameter[]
            {
        new StringHintParameter(string.Empty)
            }, null, duration);
        }
        public void SendHint(string text, HintEffect[] effects, float duration = 3f)
        {
            this.ReferenceHub.hints.Show(new TextHint(text, new HintParameter[]
            {
        new StringHintParameter(string.Empty)
            }, effects, duration));
        }
        public void SendHint(string text, HintParameter[] parameters, HintEffect[] effects = null, float duration = 3f)
        {
            HintDisplay hints = this.ReferenceHub.hints;
            HintParameter[] parameters2;
            if (!parameters.IsEmpty<HintParameter>())
            {
                parameters2 = parameters;
            }
            else
            {
                (parameters2 = new HintParameter[1])[0] = new StringHintParameter(string.Empty);
            }
            hints.Show(new TextHint(text, parameters2, effects, duration));
        }
        public void SendHint(string msg, float ShowTime, float Y)
        {
            var hint = new SSHint.SSHint
            {
                Content = msg,
                Duration = ShowTime,
                Color = UnityEngine.Color.white.ToString(),
                VerticalCoordinate = Y,
                FontSize = 25
            };
            PlayerHintManager.SendHint(ReferenceHub, hint);
        }
        public NetworkConnection Connection
        {
            get
            {
                if (!this.IsHost)
                {
                    return this.ReferenceHub.networkIdentity.connectionToClient;
                }
                return this.ReferenceHub.networkIdentity.connectionToServer;
            }
        }
        public void SendBroadcast(string msg, ushort time)
        {
            Broadcast.Singleton.TargetAddElement(this.Connection, msg, time, Broadcast.BroadcastFlags.Normal);
        }
        public NetworkAudioPlayer AddAudio(string filePath,
            Enums.AudioType audioType, bool loop = false, float volume = 1.0f, bool is3DAudio = true)
        {
            return AudioManager.PlayAudioForPlayer(ReferenceHub, filePath, audioType, loop, volume, is3DAudio);
        }
        public void StopAllAudio()
        {
            AudioManager.StopAudioForPlayer(ReferenceHub);
        }

        /// <summary>
        /// 暂停玩家的所有音频
        /// </summary>
        public void PauseAllAudio()
        {
            AudioManager.PauseAudioForPlayer(ReferenceHub);
        }

        /// <summary>
        /// 恢复玩家的所有音频
        /// </summary>
        public void ResumeAllAudio()
        {
            AudioManager.ResumeAudioForPlayer(ReferenceHub);
        }
        public bool IsPressKey(SimpleKeyBind simpleKeyBind)
        {
            bool B = IsConnected && simpleKeyBind.IsPressed();
            return B;
        }
        public bool IsPressKey(KeyCode keyCode)
        {
            bool B = IsConnected && Input.GetKeyDown(keyCode);
            return B;
        }
        public void ClearInventory()
        {

            foreach (var item in Items.ToArray())
            {
                item.UnSpawn();
            }
        }
    }
}
