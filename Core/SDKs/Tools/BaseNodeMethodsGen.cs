#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using Core.ViewModel.TaskEditor;
using PluginCore;

#endregion

namespace Core.SDKs.Tools;

public class BaseNodeMethodsGen
{
    private static readonly Dictionary<string, Type> _baseType = new()
    {
        { "字符串", typeof(string) },
        { "布尔", typeof(bool) },
        { "整型", typeof(int) },
        { "双精度浮点数", typeof(double) }
    };

    public static Dictionary<string, string> _i18n = new()
    {
        { "System.String", "字符串" },
        { "System.Boolean", "布尔" },
        { "System.Int32", "整数" },
        { "System.Double", "浮点" },
        { "System.Object", "任意" },
        { "PluginCore.NodeConnectorClass", "节点" }
    };

    public static string GetI18N(string key)
    {
        if (_i18n.TryGetValue(key, out var n))
        {
            return n;
        }

        return key;
    }

    public static void GenBaseNodeMethods(BindingList<object> nodeMethods)
    {
        foreach (var (key, value) in _baseType)
        {
            var String = new PointItem()
            {
                Plugin = "Kitopia",
                MerthodName = value.FullName,
                Title = key
            };
            ObservableCollection<ConnectorItem> StringoutItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = value,
                    Title = GetI18N(value.FullName),
                    TypeName = GetI18N(value.FullName),
                    IsOut = true
                }
            };
            String.Output = StringoutItems;
            ObservableCollection<ConnectorItem> StringinItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = value,
                    Title = GetI18N(value.FullName),
                    TypeName = GetI18N(value.FullName),
                    IsSelf = true
                }
            };
            String.Input = StringinItems;
            nodeMethods.Add(String);
        } //基本数值类型

        //if
        {
            var String = new PointItem()
            {
                Plugin = "Kitopia",
                MerthodName = "判断",
                Title = "判断"
            };
            ObservableCollection<ConnectorItem> StringoutItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    Title = "真",
                    TypeName = GetI18N(typeof(NodeConnectorClass).FullName),
                    IsOut = true
                },
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    Title = "假",
                    TypeName = GetI18N(typeof(NodeConnectorClass).FullName),
                    IsOut = true
                }
            };
            String.Output = StringoutItems;
            ObservableCollection<ConnectorItem> StringinItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    Title = "流输入",
                    TypeName = "节点"
                },
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(bool),
                    Title = GetI18N(typeof(bool).FullName),
                    TypeName = GetI18N(typeof(bool).FullName)
                }
            };
            String.Input = StringinItems;
            nodeMethods.Add(String);
        }
        //==
        {
            var String = new PointItem()
            {
                Plugin = "Kitopia",
                MerthodName = "相等",
                Title = "相等"
            };
            ObservableCollection<ConnectorItem> StringoutItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(bool),
                    Title = GetI18N(typeof(bool).FullName),
                    TypeName = GetI18N(typeof(bool).FullName),
                    IsOut = true
                }
            };
            String.Output = StringoutItems;
            ObservableCollection<ConnectorItem> StringinItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    Title = "流输入",
                    TypeName = "节点"
                },
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(object),
                    Title = GetI18N(typeof(object).FullName),
                    TypeName = GetI18N(typeof(object).FullName)
                },
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(object),
                    Title = GetI18N(typeof(object).FullName),
                    TypeName = GetI18N(typeof(object).FullName)
                }
            };
            String.Input = StringinItems;
            nodeMethods.Add(String);
        }
    }
}