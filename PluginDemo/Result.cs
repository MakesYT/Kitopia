using PluginCore.Attribute;

namespace PluginDemo;

[AutoUnbox]
public class Result
{
    public string Name
    {
        get;
        set;
    }

    public int Id
    {
        get;
        set;
    }
}