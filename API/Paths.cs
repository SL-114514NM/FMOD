using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.API
{
    public class Paths
    {
        public static readonly string AppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static readonly string BaseDir = System.IO.Path.Combine(AppData, "FMOD");
        public static readonly string DependenceDir = System.IO.Path.Combine(BaseDir, "Dependence");
        public static readonly string ConfigDir = System.IO.Path.Combine(BaseDir, "Config");
        public static readonly string PermissionsPath = System.IO.Path.Combine(ConfigDir, "Permissions.yaml");

        public static string GetPluginsDir(int serverPort)
        {
            return System.IO.Path.Combine(BaseDir, "Plugins", serverPort.ToString());
        }

        public static string GetConfigPath(int serverPort)
        {
            return System.IO.Path.Combine(ConfigDir, serverPort.ToString(), "Config.yaml");
        }

        /// <summary>
        /// 生成所有必要的文件夹和配置文件
        /// </summary>
        /// <param name="serverPort">服务器端口号</param>
        public static void GenerateFoldersAndFiles(int serverPort)
        {
            try
            {
                Log.Debug($"正在为服务器端口 {serverPort} 生成文件夹结构...");

                // 创建基础目录
                CreateDirectoryIfNotExists(BaseDir);
                CreateDirectoryIfNotExists(DependenceDir);
                CreateDirectoryIfNotExists(ConfigDir);

                // 创建端口特定的目录
                string pluginsDir = GetPluginsDir(serverPort);
                string configSubDir = System.IO.Path.Combine(ConfigDir, serverPort.ToString());
                CreatePermissionsFileIfNotExists();

                CreateDirectoryIfNotExists(pluginsDir);
                CreateDirectoryIfNotExists(configSubDir);
                Log.Debug($"文件夹和文件生成完成！");
                Log.Debug($"插件目录: {pluginsDir}");
                Log.Debug($"配置目录: {configSubDir}");
            }
            catch (Exception ex)
            {
                Log.Debug($"生成文件夹和文件时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建目录（如果不存在）
        /// </summary>
        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Log.Debug($"创建目录: {path}");
            }
        }
        private static void CreatePermissionsFileIfNotExists()
        {
            if (!File.Exists(PermissionsPath))
            {
                string defaultPermissions = @"# 权限配置文件
# 格式说明：
# - UserId: 玩家ID（SteamID64）
#   Permissions: 权限列表
#     - 权限字符串（支持正则表达式匹配）

# 示例：
# - UserId: 76561198000000000@steam
#   Permissions:
#     - music\.play
#     - music\.stop
#     - hint\.send
#
# - UserId: 76561198000000001@steam
#   Permissions:
#     - .*  # 所有权限
";
                File.WriteAllText(PermissionsPath, defaultPermissions);
                Log.Debug($"创建权限文件: {PermissionsPath}");
            }
        }
    }
}
