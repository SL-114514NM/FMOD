using FMOD.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static FMOD.API.Load;

namespace FMOD.Plugins
{
    public abstract class Plugin
    {
        /// <summary>
        /// 插件作者
        /// </summary>
        public abstract string Author { get; }

        /// <summary>
        /// 插件版本
        /// </summary>
        public abstract Version Version { get; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// 插件描述
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// 插件配置类型
        /// </summary>
        public virtual Type ConfigType { get; } = null;

        /// <summary>
        /// 插件配置实例
        /// </summary>
        public object Config { get; private set; }

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="serverPort">服务器端口号</param>
        public void Initialize(int serverPort, object config)
        {
            try
            {
                // 处理包装器配置
                if (config is EnabledPluginConfigWrapper wrapper)
                {
                    Config = wrapper.PluginConfig;
                    IsEnabled = wrapper.IsEnabled;
                }
                else if (config != null && ConfigType != null && config.GetType() == ConfigType)
                {
                    Config = config;

                    // 检查是否有 IsEnabled 属性
                    var enabledProperty = config.GetType().GetProperty("IsEnabled");
                    if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool))
                    {
                        IsEnabled = (bool)enabledProperty.GetValue(config);
                    }
                    else
                    {
                        IsEnabled = true; // 默认启用
                    }
                }
                else if (ConfigType != null)
                {
                    // 如果配置类型不匹配，使用默认配置
                    Config = Activator.CreateInstance(ConfigType);
                    IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FMOD] 插件 {Name} 初始化配置失败: {ex.Message}");
                Config = null;
                IsEnabled = true;
            }
        }
        public bool IsEnabled { get; private set; } = true;

        /// <summary>
        /// 设置插件启用状态
        /// </summary>
        public void SetEnabled(bool enabled, int serverPort)
        {
            if (IsEnabled == enabled) return;

            IsEnabled = enabled;

            // 更新配置
            if (Config != null)
            {
                var enabledProperty = Config.GetType().GetProperty("IsEnabled");
                if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool))
                {
                    enabledProperty.SetValue(Config, enabled);
                }
            }

            // 保存配置
            SaveConfig(serverPort);

            // 根据状态调用相应方法
            if (enabled)
            {
                OnEnabled();
                Console.WriteLine($"[FMOD] 已启用插件: {Name}");
            }
            else
            {
                OnDisabled();
                Console.WriteLine($"[FMOD] 已禁用插件: {Name}");
            }
        }

        /// <summary>
        /// 保存插件配置
        /// </summary>
        public void SaveConfig(int serverPort)
        {
            // 使用包装器确保包含 IsEnabled
            object configToSave;

            if (Config != null)
            {
                var enabledProperty = Config.GetType().GetProperty("IsEnabled");
                if (enabledProperty != null && enabledProperty.PropertyType == typeof(bool))
                {
                    configToSave = Config;
                }
                else
                {
                    configToSave = new EnabledPluginConfigWrapper
                    {
                        IsEnabled = IsEnabled,
                        PluginConfig = Config
                    };
                }
            }
            else
            {
                configToSave = new Load.EnabledPluginConfigWrapper
                {
                    IsEnabled = IsEnabled,
                    PluginConfig = new DefaultPluginConfig()
                };
            }

            string configPath = GetPluginConfigPath(serverPort, Name);
            string configDir = Path.GetDirectoryName(configPath);

            Directory.CreateDirectory(configDir);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            string yaml = serializer.Serialize(configToSave);
            File.WriteAllText(configPath, yaml);
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        /// <param name="serverPort">服务器端口号</param>
        private void LoadConfig(int serverPort)
        {
            string configPath = Paths.GetConfigPath(serverPort);
            string configDir = Path.GetDirectoryName(configPath);

            Directory.CreateDirectory(configDir);

            if (!File.Exists(configPath))
            {
                // 创建默认配置
                Config = Activator.CreateInstance(ConfigType);
                SaveConfig(serverPort);
            }
            else
            {
                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();

                    string yaml = File.ReadAllText(configPath);
                    Config = deserializer.Deserialize(yaml, ConfigType);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"加载配置失败: {configPath}, 错误: {ex.Message}");
                    Config = Activator.CreateInstance(ConfigType);
                }
            }
        }

        public T GetConfig<T>() where T : class, new()
        {
            return Load.GetPluginConfig<T>(Name);
        }
        private string GetPluginConfigPath(int serverPort, string pluginName)
        {
            string configDir = Path.Combine(Paths.ConfigDir, serverPort.ToString(), "Plugins");
            return Path.Combine(configDir, $"{pluginName}.yaml");
        }
        /// <summary>
        /// 插件启用时调用
        /// </summary>
        public abstract void OnEnabled();

        /// <summary>
        /// 插件禁用时调用
        /// </summary>
        public abstract void OnDisabled();
    }
}
