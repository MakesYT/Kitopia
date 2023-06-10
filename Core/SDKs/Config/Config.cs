using System.ComponentModel;
using System.Runtime.Serialization;
using Core.SDKs.HotKey;

namespace Core.SDKs.Config;

public record Config
{
    public int verInt = 0;
    public string ver = "0.0.2";
    public bool useEverything = true;
    public bool autoStart = true;
    public bool canReadClipboard = true;
    public bool debugMode = false;
    public string themeChoice = "跟随系统";
    public string themeColor = "#EC407A";
    public int maxHistory = 4;
    public int inputSmoothingMilliseconds = 50;


    public List<string> alwayShows = new();

    public List<string> customCollections = new();

    public BindingList<HotKeyModel> hotKeys = new()
    {
        new HotKeyModel
        {
            Name = "显示搜索框", IsUsable = true, IsSelectCtrl = false, IsSelectAlt = true, IsSelectWin = false,
            IsSelectShift = false, SelectKey = EKey.空格
        }
    };


    public List<string> lastOpens = new();


    public List<string> errorLnk = new();


    [OnDeserializing]
    private void OnDeserializing(StreamingContext context) //反序列化时hotkeys的默认值会被添加,需要先清空
    {
        // 清空hotKeys列表
        hotKeys.Clear();
    }
}