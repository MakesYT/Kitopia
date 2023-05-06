namespace Core.SDKs.Config;

public record Config
{
    public List<string> lastOpens = new();
    public string ver = "dev in dev";
    public int verInt = 0;
}