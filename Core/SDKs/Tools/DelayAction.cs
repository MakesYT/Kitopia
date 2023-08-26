#region

using Timer = System.Timers.Timer;

#endregion

namespace Core.SDKs.Tools;

public class DelayAction
{
    private Timer? _timerDbc;

    /// <summary>
    ///     延迟timesMs后执行。 在此期间如果再次调用，则重新计时
    /// </summary>
    /// <param name="timeMs"></param>
    /// <param name="invoker">同步对象，一般为Control控件。 如不需同步可传null</param>
    /// <param name="action"></param>
    public void Debounce(int timeMs, TaskScheduler invoker, Action action)
    {
        lock (this)
        {
            if (_timerDbc == null)
            {
                _timerDbc = new Timer(timeMs);
                _timerDbc.AutoReset = false;
                _timerDbc.Elapsed += (_, _) =>
                {
                    _timerDbc.Stop();
                    _timerDbc.Close();
                    _timerDbc = null;

                    {
                        Task.Factory.StartNew(action, CancellationToken.None, TaskCreationOptions.None, invoker)
                            .Wait();

                        //action.Invoke();
                    }
                };
            }

            _timerDbc.Stop();
            _timerDbc.Start();
        }
    }
}