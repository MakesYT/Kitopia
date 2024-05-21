using PluginCore.Attribute;

namespace KitopiaEx;

[CustomNodeInputType(typeof(NodeInputConnector1))]
public class NodeInputType1
{
    public string Name { get; set; }
}