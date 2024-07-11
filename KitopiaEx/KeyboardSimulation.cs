using System.Threading;
using System.Threading.Tasks;
using PluginCore.Attribute;
using SharpHook;
using SharpHook.Native;

namespace KitopiaEx;

public class KeyboardSimulation
{
    [PluginMethod("按下键盘按键", "key=按键")]
    public void PressKey([SelfInput] KeyCode key, CancellationToken ct)
    {
        var eventSimulator = new EventSimulator();
        eventSimulator.SimulateKeyPress(key);
    }

    [PluginMethod("释放键盘按键", "key=按键")]
    public void ReleaseKey(KeyCode key, CancellationToken ct)
    {
        var eventSimulator = new EventSimulator();
        eventSimulator.SimulateKeyRelease(key);
    }

    [PluginMethod("按下键盘按键并延迟释放", "key=按键")]
    public void PressAndReleaseKey(KeyCode key, CancellationToken ct)
    {
        PressKey(key, ct);
        Task.Delay(200).GetAwaiter().GetResult();
        ReleaseKey(key, ct);
    }
}