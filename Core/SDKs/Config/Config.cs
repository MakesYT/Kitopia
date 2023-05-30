using System.Runtime.Serialization;
using Core.SDKs.HotKey;
using Newtonsoft.Json;

namespace Core.SDKs.Config;

public record Config
{
    public List<string> alwayShows = new();
    public bool autoStart = true;
    public bool canReadClipboard = true;
    public List<string> customCollections = new();

    public List<HotKeyModel> hotKeys = new()
    {
        new HotKeyModel
        {
            Name = "显示搜索框", IsUsable = true, IsSelectCtrl = false, IsSelectAlt = true,
            IsSelectShift = false, SelectKey = EKey.Space
        }
    };

    public int inputSmoothingMilliseconds = 50;


    public List<string> lastOpens = new();
    public int maxHistory = 4;
    public string themeChoice = "跟随系统";
    public string themeColor = "#EC407A";
    public bool useEverything = true;
    public string ver = "0.0.2";
    public int verInt = 0;


    [OnDeserializing]
    private void OnDeserializing(StreamingContext context) //反序列化时hotkeys的默认值会被添加,需要先清空
    {
        // 清空hotKeys列表
        hotKeys.Clear();
    }
}