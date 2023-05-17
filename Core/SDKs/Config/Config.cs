namespace Core.SDKs.Config;

public record Config
{
    public List<string> lastOpens = new();
    public bool isDark = false;
    public bool themeFollowSys = true;
    public bool autoStart = true;
    public string ver = "dev in dev";
    public int verInt = 0;
}