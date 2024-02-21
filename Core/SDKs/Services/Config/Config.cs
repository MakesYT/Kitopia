#region

using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Core.SDKs.HotKey;
using Newtonsoft.Json;
using PluginCore;

#endregion

namespace Core.SDKs.Services.Config;

public record Config
{
    public List<string> alwayShows = new();
    public bool autoStart = true;
    public bool autoStartEverything = true;
    public bool canReadClipboard = true;
    public List<string> customCollections = new();
    public List<PluginInfo> EnabledPluginInfos = new();


    public List<string> errorLnk = new();
    public string everythingOnlyKey = "";
    public List<string> ignoreItems = new();
    public int inputSmoothingMilliseconds = 50;


    public Dictionary<string, int> lastOpens = new();
    public int maxHistory = 4;
    //截图
    public bool 截图直接复制到剪贴板 = true;

    //鼠标快捷键
    public MouseHookType mouseKey = MouseHookType.鼠标侧键2;
    public int mouseKeyInverval = 1000;
    public List<string> mouseQuickItems = new();

    public string themeChoice = "跟随系统";
    public string themeColor = "#EC407A";
    public bool useEverything = true;
    [JsonIgnore] public string ver = "0.0.2";
    [JsonIgnore] public int verInt = 0;

    public ObservableCollection<HotKeyModel> hotKeys
    {
        get;
    } = new()
    {
        new HotKeyModel
        {
            MainName = "Kitopia", Name = "显示搜索框", IsUsable = true, IsSelectCtrl = false, IsSelectAlt = true,
            IsSelectWin = false,
            IsSelectShift = false, SelectKey = EKey.空格
        },
        new HotKeyModel
        {
            MainName = "Kitopia", Name = "截图", IsUsable = true, IsSelectCtrl = true, IsSelectAlt = true,
            IsSelectWin = false,
            IsSelectShift = false, SelectKey = EKey.Q
        }
    };

    [OnDeserializing]
    // ReSharper disable once UnusedMember.Local
    // ReSharper disable once UnusedParameter.Local
    private void OnDeserializing(StreamingContext context) //反序列化时hotkeys的默认值会被添加,需要先清空
    {
        // 清空hotKeys列表
        hotKeys.Clear();
    }
}