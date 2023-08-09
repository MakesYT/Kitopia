using System.Collections.ObjectModel;
using System.ComponentModel;
using Core.ViewModel.TaskEditor;

namespace Core.SDKs.Tools;

public class BaseNodeMethodsGen
{
    static Dictionary<string, Type> _baseType = new Dictionary<string, Type>()
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
            PointItem String = new PointItem()
            {
                Plugin = "Kitopia",
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
        }
    }
}