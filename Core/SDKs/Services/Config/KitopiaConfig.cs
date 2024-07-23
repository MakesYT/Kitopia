using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Avalonia.Threading;
using Core.SDKs.HotKey;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using PluginCore;
using PluginCore.Attribute;
using PluginCore.Config;

// ReSharper disable InconsistentNaming
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace Core.SDKs.Services.Config;

[ConfigName("Kitopia主配置文件")]
public class KitopiaConfig : ConfigBase
{
    private static ILog log = LogManager.GetLogger("KitopiaConfig");
    public List<string> alwayShows = new();

    [ConfigFieldCategory("基本")] [ConfigField<ThemeEnum>("主题选择", "跟随系统,深色还是浅色?", 0xf33c)]
    public ThemeEnum themeChoice = ThemeEnum.跟随系统;

    [ConfigField("自动启动", "可能被杀毒软件阻止", 0xE61C, ConfigFieldType.布尔)]
    public bool autoStart = true;

    [ConfigField("允许程序读取剪贴板", "自动读取剪贴板路径和剪贴板图像保存依赖于此权限", 0xF2D7, ConfigFieldType.布尔)]
    public bool canReadClipboard = true;

    [ConfigFieldCategory("搜索框")] [ConfigField("搜索框快捷键", "显示搜索框快捷键", 0xF4B8, ConfigFieldType.快捷键)]
    public HotKeyModel searchHotKey = new()
    {
        MainName = "Kitopia", Name = "显示搜索框", IsSelectCtrl = false, IsSelectAlt = true,
        IsSelectWin = false,
        IsSelectShift = false, SelectKey = EKey.空格,
    };

    [JsonIgnore]
    public Action<HotKeyModel> searchHotKeyAction => e =>
    {
        log.Debug("显示搜索框热键被触发");
        ServiceManager.Services.GetService<ISearchWindowService>()!.ShowOrHiddenSearchWindow();
    };

    [ConfigField("允许程序调用Everything索引文档", "索引文档依赖于此功能", 0xF3AE, ConfigFieldType.布尔)]
    public bool useEverything = true;

    [ConfigField("自动启动Everything", "在Everything未启动时自动启动", 0xE61C, ConfigFieldType.布尔)]
    public bool autoStartEverything = true;

    [ConfigField("允许程序调用Everything索引的文件类型", "设置Everything检索的文件类型,注意已索引的项目仅当重启软件后消失", 0xf8cb, ConfigFieldType.字符串列表支持添加)]
    public ObservableCollection<string> everythingSearchExtensions =
        ["*.docx", "*.doc", "*.xls", "*.xlsx", "*.pdf", "*.ppt", "*.pptx"];


    public List<string> customCollections = new();
    public List<PluginInfo> EnabledPluginInfos = new();

    public List<string> errorLnk = new();
    public string everythingOnlyKey = "";


    [ConfigField("忽略项", "忽略指定的文件或文件夹", 0xF2D7, ConfigFieldType.字符串列表)]
    public ObservableCollection<string> ignoreItems = new();

    [ConfigField("输入平滑延时", "在指定时间内不处理数据以减轻性能消耗", 0xED9B, ConfigFieldType.整数滑块, null, 1000, 50, 10)]
    public int inputSmoothingMilliseconds = 50;

    public Dictionary<string, int> lastOpens = new();

    [ConfigField("最大历史记录", "最大历史记录数", 0xF2D7, ConfigFieldType.整数列表, null, 10, 1, 1)]
    public int maxHistory = 6;

    [ConfigFieldCategory("鼠标快捷操作")] [ConfigField<MouseHookType>("鼠标快捷键", "修改鼠标快捷菜单激活按键", 0xF4B8)]
    public MouseHookType mouseKey = MouseHookType.鼠标侧键2;

    [ConfigField("鼠标快捷键间隔", "按下按键指定时间后触发", 0xED9B, ConfigFieldType.整数滑块, null, 3000, 100, 50)]
    public int mouseKeyInverval = 1000;

    public List<string> mouseQuickItems = new();


    [ConfigFieldCategory("截图")] [ConfigField("截图直接复制到剪贴板", "截图直接复制到剪贴板,不显示工具栏", 0xE61C, ConfigFieldType.布尔)]
    public bool 截图直接复制到剪贴板 = true;

    [ConfigField("截图快捷键", "修改截图快捷键", 0xF4B8, ConfigFieldType.快捷键)]
    public HotKeyModel screenShotHotKey = new()
    {
        MainName = "Kitopia", Name = "截图", IsSelectCtrl = true, IsSelectAlt = true,
        IsSelectWin = false,
        IsSelectShift = false, SelectKey = EKey.Q,
    };

    [JsonIgnore]
    public Action<HotKeyModel> screenShotHotKeyAction => e =>
    {
        log.Debug("截图热键被触发");
        Dispatcher.UIThread.InvokeAsync(() =>
            {
                ServiceManager.Services.GetService<IScreenCaptureWindow>()!.CaptureScreen();
            })
            .GetTask()
            .ContinueWith((e) =>
            {
                if (e.IsFaulted)
                {
                    log.Error(e.Exception);
                    ServiceManager.Services.GetService<IErrorWindow>()!.ShowErrorWindow(
                        "截图失败", e.Exception.Message);
                }
            });
    };
}