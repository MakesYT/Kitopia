#region

using PluginCore.Attribute;

#endregion

namespace PluginDemo;

[AutoUnbox]
public class Result
{
    public Result(string name, int id)
    {
        Name = name;
        Id = id;
    }

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