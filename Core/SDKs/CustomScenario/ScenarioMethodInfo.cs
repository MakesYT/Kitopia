using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Core.SDKs.Services.Plugin;
using Core.SDKs.Tools;
using PluginCore;
using PluginCore.Attribute;
using PluginCore.Attribute.Scenario;

namespace Core.SDKs.CustomScenario;

public class ScenarioMethodInfo
{
    public ScenarioMethodInfo(MethodInfo method, PluginInfo pluginInfo, ScenarioMethodAttribute attribute)
    {
        Method = method;
        PluginInfo = pluginInfo;
        Attribute = attribute;
    }

    public MethodInfo Method { get; }
    public PluginInfo PluginInfo { get; }
    public ScenarioMethodAttribute Attribute { get; }

    public string MethodAbsolutelyName
    {
        get
        {
            StringBuilder sb = new StringBuilder("|");

            foreach (var genericArgument in Method.GetParameters())
            {
                var plugin = PluginManager.EnablePlugin
                    .FirstOrDefault((e) => e.Value._dll == genericArgument.ParameterType.Assembly)
                    .Value;
                // type.Assembly.
                // var a = PluginManager.GetPlugnNameByTypeName(type.FullName);
                if (plugin is null)
                {
                    sb.Append($"System {genericArgument.ParameterType.FullName}");
                    sb.Append("|");
                    continue;
                }

                sb.Append($"{plugin.ToPlgString()} {genericArgument.ParameterType.FullName}");
                sb.Append("|");
            }

            sb.Remove(sb.Length - 1, 1);
            return $"{PluginInfo.Author}_{PluginInfo.PluginId}#{Method.DeclaringType!.FullName}#{Method.Name}{sb}";
        }
    }

    public string MethodRawTitle =>
        $"{PluginInfo.Author}_{PluginInfo.PluginId}#{Method.DeclaringType!.FullName}#{Method.Name}";

    public string MethodTitle =>
        $"{PluginInfo.Author}_{PluginInfo.PluginId}#{Method.DeclaringType!.FullName}#{Attribute.Name}";

    public ScenarioMethodNode GenerateNode()
    {
        var pointItem = new ScenarioMethodNode()
        {
            ScenarioMethodInfo = this,
            Title = MethodTitle
        };
        ObservableCollection<ConnectorItem> inpItems = new();
        inpItems.Add(new ConnectorItem()
        {
            Source = pointItem,
            Type = typeof(NodeConnectorClass),
            Title = "流输入",
            TypeName = "节点"
        });
        int autoUnboxIndex = 0;
        for (var index = 0;
             index < Method.GetParameters()
                 .Length;
             index++)
        {
            var parameterInfo = Method.GetParameters()[index];
            if (parameterInfo.ParameterType.FullName == "System.Threading.CancellationToken")
            {
                continue;
            }

            bool IsSelf = parameterInfo.GetCustomAttributes(typeof(SelfInput))
                .Any();

            if (parameterInfo.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
            {
                autoUnboxIndex++;
                var type = parameterInfo.ParameterType;
                foreach (var memberInfo in type.GetProperties())
                {
                    List<string>? interfaces = null;
                    if (!memberInfo.PropertyType.FullName.StartsWith("System."))
                    {
                        interfaces = new();
                        foreach (var @interface in memberInfo.PropertyType.GetInterfaces())
                        {
                            interfaces.Add(@interface.FullName);
                        }
                    }

                    inpItems.Add(new ConnectorItem()
                    {
                        Source = pointItem,
                        Type = memberInfo.PropertyType,
                        IsSelf = IsSelf,
                        AutoUnboxIndex = autoUnboxIndex,
                        Interfaces = interfaces,
                        Title = Attribute.GetParameterName(memberInfo.Name),
                        TypeName = BaseNodeMethodsGen.GetI18N(memberInfo.PropertyType.FullName),
                    });
                }
            }
            else
            {
                var connectorItem = new ConnectorItem()
                {
                    Source = pointItem,
                    Type = parameterInfo.ParameterType,
                    IsSelf = IsSelf,
                    Title = Attribute.GetParameterName(parameterInfo.Name),
                    TypeName = BaseNodeMethodsGen.GetI18N(parameterInfo.ParameterType.FullName)
                };
                if (parameterInfo.ParameterType.GetCustomAttribute<CustomNodeInputType>() is not null
                    and var customNodeInputType)
                {
                    connectorItem.isPluginInputConnector = true;
                    connectorItem.IsSelf = true;
                    try
                    {
                        var service = PluginManager.EnablePlugin[PluginInfo.ToPlgString()].ServiceProvider
                            .GetService(customNodeInputType.Type);
                        connectorItem.PluginInputConnector = service as INodeInputConnector;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }

                inpItems.Add(connectorItem);
            }

            //Log.Debug($"参数{index}:类型为{parameterInfo.ParameterType}");
        }

        if (Method.ReturnParameter.ParameterType != typeof(void))
        {
            ObservableCollection<ConnectorItem> outItems = new();
            if (Method.ReturnParameter.ParameterType.GetCustomAttribute(typeof(AutoUnbox)) is not null)
            {
                autoUnboxIndex++;
                var type = Method.ReturnParameter.ParameterType;
                foreach (var memberInfo in type.GetProperties())
                {
                    List<string>? interfaces = null;
                    if (!memberInfo.PropertyType.FullName.StartsWith("System."))
                    {
                        interfaces = new();
                        foreach (var @interface in memberInfo.PropertyType.GetInterfaces())
                        {
                            interfaces.Add(@interface.FullName);
                        }
                    }

                    outItems.Add(new ConnectorItem()
                    {
                        Source = pointItem,
                        Type = memberInfo.PropertyType,
                        AutoUnboxIndex = autoUnboxIndex,
                        Interfaces = interfaces,
                        Title = Attribute.GetParameterName(memberInfo.Name),
                        TypeName = BaseNodeMethodsGen.GetI18N(memberInfo.PropertyType.FullName),
                        IsOut = true
                    });
                }
            }
            else
            {
                List<string> interfaces = new();
                foreach (var @interface in Method.ReturnParameter.ParameterType.GetInterfaces())
                {
                    interfaces.Add(@interface.FullName);
                }


                outItems.Add(new ConnectorItem()
                {
                    Source = pointItem,
                    Type = Method.ReturnParameter.ParameterType,
                    Title = Attribute.GetParameterName("return"),
                    Interfaces = interfaces,
                    TypeName =
                        BaseNodeMethodsGen.GetI18N(Method.ReturnParameter.ParameterType.FullName),
                    IsOut = true
                });
            }


            pointItem.Output = outItems;
        }


        pointItem.Input = inpItems;

        return pointItem;
    }
}