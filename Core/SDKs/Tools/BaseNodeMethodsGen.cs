#region

using System.Collections.ObjectModel;
using System.ComponentModel;
using Core.SDKs.CustomScenario;
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
        { "双精度浮点数", typeof(double) },
    };

    public static Dictionary<string, string> _i18n = new()
    {
        { "System.String", "字符串" },
        { "System.Boolean", "布尔" },
        { "System.Int32", "整数" },
        { "System.Double", "浮点" },
        { "System.Object", "任意" },
        { "PluginCore.NodeConnectorClass", "节点" },
    };

    public static string GetI18N(string key)
    {
        if (_i18n.TryGetValue(key, out var n))
        {
            return n;
        }

        return key;
    }

    public static void GenBaseNodeMethods(BindingList<BindingList<object>> nodeMethods)
    {
        var nodes = new BindingList<object>();
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
                    InputObject = value.IsValueType ? Activator.CreateInstance(value) : null,
                    Title = GetI18N(value.FullName),
                    TypeName = GetI18N(value.FullName),
                    IsSelf = true
                }
            };
            if (value.FullName == "System.Int32")
            {
                StringinItems[0].InputObject = (double)0;
            }

            String.Input = StringinItems;
            nodes.Add(String);
        } //基本数值类型

        //SearchItem
        var point = new PointItem()
        {
            Plugin = "Kitopia",
            MerthodName = "本地项目",
            Title = "本地项目"
        };
        ObservableCollection<ConnectorItem> pointOutItems = new()
        {
            new ConnectorItem()
            {
                Source = point,
                Type = typeof(string),
                Title = "本地项目",
                TypeName = "字符串",
                IsOut = true
            }
        };
        point.Output = pointOutItems;
        ObservableCollection<ConnectorItem> pointInItems = new()
        {
            new ConnectorItem()
            {
                Source = point,
                Type = typeof(string),
                RealType = typeof(SearchViewItem),
                InputObject = "",
                Title = "本地项目",
                TypeName = "字符串",
                IsSelf = true
            }
        };
        point.Input = pointInItems;
        nodes.Add(point);
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
            nodes.Add(String);
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
            nodes.Add(String);
        }
        //1 to 2
        {
            var String = new PointItem()
            {
                Plugin = "Kitopia",
                MerthodName = "一对二",
                Title = "一对二"
            };
            ObservableCollection<ConnectorItem> StringoutItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    Title = "流输出",
                    IsOut = true,
                    TypeName = "节点"
                },
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    IsOut = true,
                    Title = "流输出",
                    TypeName = "节点"
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
                }
            };
            String.Input = StringinItems;
            nodes.Add(String);
        }
        //1 to n
        {
            var String = new PointItem()
            {
                Plugin = "Kitopia",
                MerthodName = "一对N",
                Title = "一对N"
            };
            ObservableCollection<ConnectorItem> StringoutItems = new()
            {
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    Title = "流输出",
                    IsOut = true,
                    TypeName = "节点"
                },
                new ConnectorItem()
                {
                    Source = String,
                    Type = typeof(NodeConnectorClass),
                    IsOut = true,
                    Title = "流输出",
                    TypeName = "节点"
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
                    Type = typeof(int),
                    InputObject = (double)2,
                    IsSelf = true,
                    SelfInputAble = false,
                    Title = "输出数量",
                    TypeName = "整数"
                }
            };
            String.Input = StringinItems;
            nodes.Add(String);
        }
        nodeMethods.Add(nodes);
    }
}