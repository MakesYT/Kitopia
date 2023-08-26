#region

using System.Windows;
using PluginCore.Attribute;

#endregion

namespace PluginDemo;

public class Test1
{
    [PluginMethod("测试代码1", $"{nameof(id)}=参数1", $"{nameof(id2)}=参数2", "Id=ID", "Name=名字", "return=返回参数")]
    public B t(string id, string id2)
    {
        MessageBox.Show("5a");
        return new B();
    }

    [PluginMethod("测试代码2", $"{nameof(id)}=参数1", "return=返回参数")]
    public void t2(Abase id) => MessageBox.Show("5a");

    [ConfigField("名称", "设置名称")] public string name = "1";
}

public interface Abase
{
}

public class B : Abase
{
}