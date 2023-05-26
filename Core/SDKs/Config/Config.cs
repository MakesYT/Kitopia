using Core.SDKs.HotKey;

namespace Core.SDKs.Config;

public record Config
{
    public int verInt = 0;
    public string ver = "dev in dev";
    public string themeChoice = "跟随系统";
    public string themeColor = "#EC407A";
    public bool autoStart = true;
    public bool useEverything = true;
    public bool canReadClipboard = true;
    public int maxHistory = 4;
    public List<string> lastOpens = new();
    public List<HotKeyModel> hotKeys = new()
    {
        new HotKeyModel
        {
            Name = "显示搜索框", IsUsable = true, IsSelectCtrl = false, IsSelectAlt = true,
            IsSelectShift = false, SelectKey = EKey.Space
        }
    };

}