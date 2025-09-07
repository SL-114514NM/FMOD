using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace FMOD.API
{
    public class Permissions
    {
        /// <summary>
        /// 权限配置模型
        /// </summary>
        public class PermissionConfig
        {
            public string UserId { get; set; } = string.Empty;
            public List<string> Permissions { get; set; } = new List<string>();
        }

        /// <summary>
        /// 玩家权限信息
        /// </summary>
        public class PlayerPermissions
        {
            public string UserId { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public List<string> Permissions { get; set; } = new List<string>();
            public DateTime LastUpdated { get; set; } = DateTime.Now;
        }
        private static readonly Dictionary<string, PlayerPermissions> PlayerPermissionsCache = new Dictionary<string, PlayerPermissions>();
        private static List<PermissionConfig> _permissionConfigs = new List<PermissionConfig>();
        private static DateTime _lastConfigLoadTime = DateTime.MinValue;
        private static readonly object _lock = new object();
        public static void Initialize()
        {
            LoadPermissionsConfig();
            Console.WriteLine("权限系统初始化完成");
        }

        /// <summary>
        /// 加载权限配置文件
        /// </summary>
        private static void LoadPermissionsConfig()
        {
            lock (_lock)
            {
                try
                {
                    if (!File.Exists(Paths.PermissionsPath))
                    {
                        Log.Debug("权限配置文件不存在，将创建默认文件");
                        Paths.GenerateFoldersAndFiles(Server.Port); // 使用默认端口创建文件
                    }

                    string yamlContent = File.ReadAllText(Paths.PermissionsPath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    _permissionConfigs = deserializer.Deserialize<List<PermissionConfig>>(yamlContent) ?? new List<PermissionConfig>();
                    _lastConfigLoadTime = DateTime.Now;

                    Log.Debug($"已加载 {_permissionConfigs.Count} 个权限配置");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载权限配置文件失败: {ex.Message}");
                    _permissionConfigs = new List<PermissionConfig>();
                }
            }
        }
        /// <summary>
        /// 重新加载权限配置
        /// </summary>
        public static void ReloadPermissions()
        {
            LoadPermissionsConfig();
            PlayerPermissionsCache.Clear();
            Log.Debug("权限配置已重新加载");
        }
        /// <summary>
        /// 保存权限配置到文件
        /// </summary>
        private static void SavePermissionsConfig()
        {
            lock (_lock)
            {
                try
                {
                    var serializer = new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    string yamlContent = serializer.Serialize(_permissionConfigs);
                    File.WriteAllText(Paths.PermissionsPath, yamlContent);
                    _lastConfigLoadTime = DateTime.Now;

                    Console.WriteLine("权限配置已保存");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"保存权限配置失败: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 获取玩家ID（优先使用SteamID，如果没有则使用UserId）
        /// </summary>
        private static string GetPlayerId(ReferenceHub player)
        {
            if (player == null) return string.Empty;

            // 优先使用SteamID
            if (!string.IsNullOrEmpty(player.authManager.UserId) && player.authManager.UserId.Contains("@steam"))
            {
                return player.authManager.UserId;
            }

            // 如果没有SteamID，使用UserId
            return player.authManager.UserId;
        }

        /// <summary>
        /// 获取玩家权限
        /// </summary>
        private static PlayerPermissions GetPlayerPermissions(ReferenceHub player)
        {
            string playerId = GetPlayerId(player);
            if (string.IsNullOrEmpty(playerId))
            {
                return new PlayerPermissions { UserId = "unknown", DisplayName = "Unknown Player" };
            }

            // 检查缓存
            if (PlayerPermissionsCache.TryGetValue(playerId, out var cachedPermissions) &&
                (DateTime.Now - cachedPermissions.LastUpdated).TotalMinutes < 5)
            {
                return cachedPermissions;
            }

            // 从配置中获取权限
            var config = _permissionConfigs.FirstOrDefault(p => p.UserId == playerId);
            var permissions = new PlayerPermissions
            {
                UserId = playerId,
                DisplayName = player.nicknameSync?.MyNick ?? "Unknown",
                LastUpdated = DateTime.Now
            };

            if (config != null)
            {
                permissions.Permissions = new List<string>(config.Permissions);
            }

            // 更新缓存
            PlayerPermissionsCache[playerId] = permissions;
            return permissions;
        }

        /// <summary>
        /// 检查玩家是否拥有指定权限
        /// </summary>
        public static bool CheckPermission(ReferenceHub player, string permission)
        {
            if (player == null) return false;
            if (string.IsNullOrEmpty(permission)) return false;

            // 自动重新加载配置（如果文件被修改）
            if ((DateTime.Now - _lastConfigLoadTime).TotalSeconds > 30)
            {
                var lastWriteTime = File.GetLastWriteTime(Paths.PermissionsPath);
                if (lastWriteTime > _lastConfigLoadTime)
                {
                    LoadPermissionsConfig();
                }
            }

            var playerPermissions = GetPlayerPermissions(player);

            // 检查权限匹配（支持正则表达式）
            foreach (var playerPermission in playerPermissions.Permissions)
            {
                try
                {
                    if (Regex.IsMatch(permission, playerPermission))
                    {
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                    // 如果正则表达式无效，进行精确匹配
                    if (permission == playerPermission)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 为玩家添加权限
        /// </summary>
        public static bool AddPermission(ReferenceHub player, string permission)
        {
            if (player == null || string.IsNullOrEmpty(permission))
                return false;

            string playerId = GetPlayerId(player);
            if (string.IsNullOrEmpty(playerId))
                return false;

            lock (_lock)
            {
                var config = _permissionConfigs.FirstOrDefault(p => p.UserId == playerId);
                if (config == null)
                {
                    // 创建新的权限配置
                    config = new PermissionConfig
                    {
                        UserId = playerId,
                        Permissions = new List<string> { permission }
                    };
                    _permissionConfigs.Add(config);
                }
                else
                {
                    // 添加权限（如果不存在）
                    if (!config.Permissions.Contains(permission))
                    {
                        config.Permissions.Add(permission);
                    }
                }

                // 更新缓存
                if (PlayerPermissionsCache.TryGetValue(playerId, out var cachedPermissions))
                {
                    if (!cachedPermissions.Permissions.Contains(permission))
                    {
                        cachedPermissions.Permissions.Add(permission);
                    }
                    cachedPermissions.LastUpdated = DateTime.Now;
                }

                SavePermissionsConfig();
                Console.WriteLine($"为玩家 {player.nicknameSync?.MyNick} 添加权限: {permission}");
                return true;
            }
        }

        /// <summary>
        /// 为玩家移除权限
        /// </summary>
        public static bool RemovePermission(ReferenceHub player, string permission)
        {
            if (player == null || string.IsNullOrEmpty(permission))
                return false;

            string playerId = GetPlayerId(player);
            if (string.IsNullOrEmpty(playerId))
                return false;

            lock (_lock)
            {
                var config = _permissionConfigs.FirstOrDefault(p => p.UserId == playerId);
                if (config != null)
                {
                    bool removed = config.Permissions.Remove(permission);
                    if (removed)
                    {
                        // 更新缓存
                        if (PlayerPermissionsCache.TryGetValue(playerId, out var cachedPermissions))
                        {
                            cachedPermissions.Permissions.Remove(permission);
                            cachedPermissions.LastUpdated = DateTime.Now;
                        }

                        SavePermissionsConfig();
                        Console.WriteLine($"移除玩家 {player.nicknameSync?.MyNick} 的权限: {permission}");
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 移除玩家的所有权限
        /// </summary>
        public static bool RemoveAllPermissions(ReferenceHub player)
        {
            if (player == null) return false;

            string playerId = GetPlayerId(player);
            if (string.IsNullOrEmpty(playerId))
                return false;

            lock (_lock)
            {
                var config = _permissionConfigs.FirstOrDefault(p => p.UserId == playerId);
                if (config != null)
                {
                    _permissionConfigs.Remove(config);

                    // 更新缓存
                    if (PlayerPermissionsCache.TryGetValue(playerId, out var cachedPermissions))
                    {
                        cachedPermissions.Permissions.Clear();
                        cachedPermissions.LastUpdated = DateTime.Now;
                    }

                    SavePermissionsConfig();
                    Console.WriteLine($"移除玩家 {player.nicknameSync?.MyNick} 的所有权限");
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 获取玩家的所有权限
        /// </summary>
        public static List<string> GetPlayerPermissionsList(ReferenceHub player)
        {
            if (player == null) return new List<string>();

            var permissions = GetPlayerPermissions(player);
            return new List<string>(permissions.Permissions);
        }

        /// <summary>
        /// 检查玩家是否拥有任何权限
        /// </summary>
        public static bool HasAnyPermission(ReferenceHub player)
        {
            if (player == null) return false;

            var permissions = GetPlayerPermissions(player);
            return permissions.Permissions.Count > 0;
        }

        /// <summary>
        /// 获取所有权限配置
        /// </summary>
        public static List<PermissionConfig> GetAllPermissions()
        {
            lock (_lock)
            {
                return new List<PermissionConfig>(_permissionConfigs);
            }
        }

        /// <summary>
        /// 清空权限缓存
        /// </summary>
        public static void ClearCache()
        {
            PlayerPermissionsCache.Clear();
            Console.WriteLine("权限缓存已清空");
        }
    }
}
