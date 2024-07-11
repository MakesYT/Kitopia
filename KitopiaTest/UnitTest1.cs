using SharpHook;
using SharpHook.Native;

namespace KitopiaTest;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var eventSimulator = new EventSimulator();
        eventSimulator.SimulateKeyPress(KeyCode.VcLeftControl);

        eventSimulator.SimulateKeyPress(KeyCode.VcLeftAlt);
        eventSimulator.SimulateKeyPress(KeyCode.VcA);
        Task.Delay(100).GetAwaiter().GetResult();

        eventSimulator.SimulateKeyRelease(KeyCode.VcA);
        eventSimulator.SimulateKeyRelease(KeyCode.VcLeftAlt);
        eventSimulator.SimulateKeyRelease(KeyCode.VcLeftControl);


        Assert.Pass();
    }
}