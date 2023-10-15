using System.Windows;
using PluginCore.Attribute;

namespace PluginDemo;

public class Test
{
    [PluginMethod("测试代码3", $"{nameof(id)}=参数1", $"{nameof(id2)}=参数2", $"{nameof(id3)}=参数3", "Id=ID", "Name=名字",
        "return=返回参数")]
    public Result t(string id, string id2, int id3)
    {
        return new Result(name: $"{id}_{id2}", id: id3);
    }

    [PluginMethod("测试代码4", $"{nameof(id)}=参数1", "return=返回参数")]
    public void t(string id2, Result id, int id3)
    {
        MessageBox.Show(id.Name + " " + id.Id + " " + id3);
    }
}