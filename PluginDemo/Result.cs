#region

using PluginCore.Attribute;

#endregion

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