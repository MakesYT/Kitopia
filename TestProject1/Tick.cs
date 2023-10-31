using System.Reflection;
using Core.SDKs.Tools;
using log4net;
using log4net.Config;

namespace TestProject1;

[TestClass]
public class Tick
{
    private static readonly ILog log = LogManager.GetLogger(nameof(Tick));
    private int d = 0;

    [TestMethod]
    public void TestMethod1()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var logConfigStream = assembly.GetManifestResourceStream("Kitopia.log4net.config")!;

        XmlConfigurator.Configure(logConfigStream);
        log.Debug(d + "  " + DateTime.Now.Millisecond);
        TickUtil tickUtil = new TickUtil(0, 100000, 1, fun);
        tickUtil.Open();
        while (true)
        {
        }
    }

    private void fun(object sender, long JumpPeriod, long interval)
    {
        log.Debug(d + "  " + DateTime.Now.Millisecond);
        d++;
    }
}