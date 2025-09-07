using CustomPlayerEffects;
using NorthwoodLib.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FMOD.API
{
    public class Server
    {
        public static int PlayerCount => Player.List.Count;
        public static int MaxPlayerCount => CustomNetworkManager.slots;
        public static int Port => ServerStatic.ServerPort;
        public static string IP => ServerConsole.Ip;
        public static List<Player> RemoteAdmins => Player.List.Where(x => x.RemoteAdminAccess).ToList();
        public static List<Player> DummyCount => Player.List.Where(x => x.IsDummy).ToList();
        public static string Passcode => ServerConsole.Password;
        public static string PublicKey => ServerConsole.PublicKey.ToString();
        public static bool IsPlayerBanned(string value)
        {
            return !string.IsNullOrEmpty(value) && (value.Contains("@") ? BanHandler.GetBan(value, BanHandler.BanType.UserId) : BanHandler.GetBan(value, BanHandler.BanType.IP)) != null;
        }
        public static bool UnbanIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || !Server.IsPlayerBanned(ipAddress))
            {
                return false;
            }
            BanHandler.RemoveBan(ipAddress, BanHandler.BanType.IP, false);
            return true;
        }
        public static void RunCommand(string Command)
        {
            ServerConsole.EnterCommand(Command);
        }
        public static string ServerName => ServerConsole.ServerName;
        public static bool UnbanUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Server.IsPlayerBanned(userId))
            {
                return false;
            }
            BanHandler.RemoveBan(userId, BanHandler.BanType.UserId, false);
            return true;
        }
        public static List<BanDetails> GetAllBannedPlayers(BanHandler.BanType banType)
        {
            return BanHandler.GetBans(banType);
        }
        public static List<BanDetails> GetAllBannedPlayers()
        {
            List<BanDetails> list = ListPool<BanDetails>.Shared.Rent();
            list.AddRange(BanHandler.GetBans(BanHandler.BanType.UserId));
            list.AddRange(BanHandler.GetBans(BanHandler.BanType.IP));
            return list;
        }

        public static short MaxTps
        {
            get
            {
                return ServerStatic.ServerTickrate;
            }
            set
            {
                ServerStatic.ServerTickrate = value;
            }
        }
        public static float SpawnProtectDuration
        {
            get
            {
                return SpawnProtected.SpawnDuration;
            }
            set
            {
                SpawnProtected.SpawnDuration = value;
            }
        }
        public static double Tps
        {
            get
            {
                return Math.Round((double)(1f / Time.smoothDeltaTime));
            }
        }
        public static bool FriendlyFire
        {
            get
            {
                return ServerConsole.FriendlyFire;
            }
            set
            {
                ServerConsole.FriendlyFire = value;
            }
        }
    }
}
