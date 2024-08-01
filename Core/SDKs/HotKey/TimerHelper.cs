using System.Timers;
using Core.SDKs.HotKey;
using Timer = System.Timers.Timer;

namespace Kitopia.SDKs;

public class TimerHelper
{
    Action<HotKeyModel> action;
    private HotKeyModel HotKeyModel;
    Timer timer;

    public TimerHelper(int interval, Action<HotKeyModel> action, HotKeyModel hotKeyModel)
    {
        timer = new Timer(interval);
        timer.AutoReset = false;
        timer.Elapsed += OnTimerElapsed;
        this.action = action;
        this.HotKeyModel = hotKeyModel;
    }

// 当计时器触发时执行的操作
    private void OnTimerElapsed(object source, ElapsedEventArgs e)
    {
        ThreadPool.QueueUserWorkItem((e) => { action.Invoke(HotKeyModel); });
    }

// 在需要开始计时器的地方调用该方法
    public void StartTimer()
    {
        timer.Start();
    }

// 在需要停止计时器的地方调用该方法
    public void StopTimer()
    {
        timer.Stop();
    }
}