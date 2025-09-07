# Select a readme language(选择自述语言)
| 语言 | Language | 链接/Link |
| :-- | :-- | :-- |
| 中文 | Chinese | [中文自述](README.md#chinese%E4%B8%AD%E6%96%87%E8%87%AA%E8%BF%B0) |
| English | English | [English README](README.md#english%E8%8B%B1%E6%96%87%E8%87%AA%E8%BF%B0) |
# English英文自述
# FMOD  
A lightweight plugin loading system for SCP: Secret Laboratory (SCP SL) in Chinese.  

# Installation  
Download "FMOD.zip" and extract it to `%appdata%` (using Windows as an example). 
After extraction, copy the "FMOD" and "SCP Secret Laboratory" folders to `%appdata%`.  
Restart your server.  
If "Welcome to FMOD" is displayed, the installation was successful.  


# Plugin Development  
[Click here to view](https://github.com/SL-114514NM/FMOD/blob/main/README.md#%E5%BC%80%E5%8F%91%E6%95%99%E7%A8%8B--development-tutorial)  

# Commands  
The built-in command is `fmod`.  
Subcommands:  
- `list` — View loaded plugins.  
- `version` — Check the FMOD version.  
- `reload` — Hot-reload the framework and plugins.
# Chinese中文自述
# FMOD
一个SCP SL的中文轻量级别的插件加载系统
# 安装
下载"FMOD.zip"然后复制到%appdata%里(以Windows为例)\
(如果无法在github上下载东西或下载太慢的大陆小伙伴，可以访问[123Pan](https://www.123912.com/s/rgcZjv-roy5d)下载)\
解压然后将里面的"FMOD","SCP Secret Laboratory"文件夹复制到%appdata%里\
重启服务器\
显示"欢迎使用FMOD"表示安装成功
# 制作插件
[点击查看](https://github.com/SL-114514NM/FMOD/blob/main/README.md#%E5%BC%80%E5%8F%91%E6%95%99%E7%A8%8B--development-tutorial)
# 命令
内置了一个命令"fmod"\
子命令:\
list --查看已加载的插件\
version --查看fmod版本\
reload --热重启框架和插件





# 开发教程 / Development Tutorial

| 语言 | Language | 链接 |
| :-- | :-- | :-- |
| 中文 | Chinese | [中文教程](#中文教程chinese-tutorial) |
| English | English | [English Tutorial](#english-tutorial-英文教程) |

---

# 中文教程(Chinese Tutorial)

## 开发步骤

### 1. 引用DLL文件
由于尚未配置NuGet包，需要手动引用以下DLL文件：
- `FMOD.dll`
- 其他相关依赖项

### 2. 创建配置文件 (Config.cs)
```csharp
public class Config
{
    // IsEnabled属性会自动生成，无需手动声明
    public string Hello { get; set; } = "Hello World";
}
```

### 3. 创建插件主类 (Plugin.cs)
```csharp
using FMOD.Plugins;

public class Plugin : PluginBase
{
    public override string Author => "开发者名称";
    public override Version Version => new Version(1, 0);
    public override string Name => "插件名称";
    public override string Description => "插件功能描述";
    public override Type ConfigType => typeof(Config);
    
    public override void OnDisabled()
    {
        // 在此处注销事件和释放资源
    }

    public override void OnEnabled()
    {
        // 插件启用时的初始化代码
        Log.Debug("插件启动成功!");
        
        // 获取配置文件实例
        var config = GetConfig<Config>();
        
        // 输出调试信息
        Log.Debug($"{config.Hello}");
        
        // 输出自定义颜色的信息
        Log.CustomInfo($"{config.Hello}", Color.red);
    }
}
```

### 4. 编译和部署
1. 按 `F6` 编译项目生成DLL文件
2. 将生成的插件文件放置在：`%appdata%/FMOD/Plugins/(服务器端口号)/` 目录下
3. 重启服务器或执行命令 `fmod reload` 进行热重载

---

# English Tutorial (英文教程)

## Development Steps

### 1. Reference DLL Files
Since NuGet packages are not yet configured, you need to manually reference the following DLL files:
- `FMOD.dll`
- Other related dependencies

### 2. Create Configuration File (Config.cs)
```csharp
public class Config
{
    // The IsEnabled property is automatically generated, no need to declare it manually
    public string Hello { get; set; } = "Hello World";
}
```

### 3. Create Main Plugin Class (Plugin.cs)
```csharp
using FMOD.Plugins;

public class Plugin : PluginBase
{
    public override string Author => "Developer Name";
    public override Version Version => new Version(1, 0);
    public override string Name => "Plugin Name";
    public override string Description => "Plugin description";
    public override Type ConfigType => typeof(Config);
    
    public override void OnDisabled()
    {
        // Unregister events and release resources here
    }

    public override void OnEnabled()
    {
        // Initialization code when plugin is enabled
        Log.Debug("Plugin started successfully!");
        
        // Get configuration instance
        var config = GetConfig<Config>();
        
        // Output debug information
        Log.Debug($"{config.Hello}");
        
        // Output custom colored message
        Log.CustomInfo($"{config.Hello}", Color.red);
    }
}
```

### 4. Compilation and Deployment
1. Press `F6` to compile the project and generate DLL files
2. Place the generated plugin files in: `%appdata%/FMOD/Plugins/(server-port-number)/` directory
3. Restart the server or execute the command `fmod reload` for hot reloading

---

## Important Notes
- Ensure all required DLL references are properly added to your project
- Test plugins in a development environment before deploying to production
- Use `Log.Debug()` for debugging purposes and `Log.CustomInfo()` for colored output
- The configuration system automatically handles enabling/disabling of plugins
