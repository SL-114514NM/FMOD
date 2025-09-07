using CommandSystem;
using FMOD.API;
using FMOD.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMOD.Commands.GAMECommand
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class FMODCommand : ICommand
    {
        public string Command => "fmod";

        public string[] Aliases => Array.Empty<string>();

        public string Description => "Description of FMOD";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = "用法: fmod <list|version|reload>\n";
                response += "  list    - 显示已加载的插件列表\n";
                response += "  version - 显示 FMOD 版本信息\n";
                response += "  reload  - 重新加载所有插件\n";
                return false;
            }
            string subCommand = arguments.ElementAt(0);

            switch (subCommand)
            {
                case "list":
                    response = ListPlugins();
                    return true;

                case "version":
                    response = ShowVersion();
                    return true;

                case "reload":
                    response = ReloadPlugins();
                    return true;

                default:
                    response = $"未知子命令: {subCommand}\n";
                    response += "可用子命令: list, version, reload";
                    return false;
            }
        }
        private string ListPlugins()
        {
            try
            {
                var plugins = Load.GetLoadedPlugins();
                int pluginCount = plugins.Count;

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"------({pluginCount})个插件已加载------");

                if (pluginCount == 0)
                {
                    sb.AppendLine("  没有加载任何插件");
                }
                else
                {
                    foreach (var plugin in plugins)
                    {
                        sb.AppendLine($"{plugin.Name} - {plugin.Description} - {plugin.Author} - v{plugin.Version}");
                    }
                }

                sb.AppendLine("----------------------------");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string ShowVersion()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== FMOD 插件框架 ===");
                sb.AppendLine($"版本: v{FMODVersion.MainVersion.ToString()}");
                sb.AppendLine($"作者: HUI");
                sb.AppendLine("====================");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        private string ReloadPlugins()
        {
            try
            {

                int serverPort = Server.Port;

                // 禁用所有插件
                Load.DisableAllPlugins();

                // 重新加载插件
                Load.LoadAllPlugins(serverPort);

                // 获取重新加载后的插件数量
                var plugins = Load.GetLoadedPlugins();

                return$"插件重新加载完成！已加载 {plugins.Count} 个插件";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
