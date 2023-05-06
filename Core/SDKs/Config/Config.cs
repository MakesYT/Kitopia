namespace Core.SDKs.Config;

public record Config
{
    public string ver = "dev in dev";
    public int verInt = 0;
    public List<string> lastOpens=new List<string>();
    
}