using Core.SDKs.HotKey;

namespace Core.SDKs.Config;

public record Config
{
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
    public string ver = "dev in dev";
    public int verInt = 0;
}