﻿using System.ComponentModel;

namespace Core.SDKs.Tools
{
    public class DelayAction
    {
        private System.Timers.Timer? _timerDbc;
        System.Timers.Timer _timerTrt;

        /// <summary>
        /// 延迟timesMs后执行。 在此期间如果再次调用，则重新计时
        /// </summary>
        /// <param name="invoker">同步对象，一般为Control控件。 如不需同步可传null</param>
        public void Debounce(int timeMs, TaskScheduler invoker, Action action)
        {
            lock (this)
            {
                if (_timerDbc == null)
                {
                    _timerDbc = new System.Timers.Timer(timeMs);
                    _timerDbc.AutoReset = false;
                    _timerDbc.Elapsed += (o, e) =>
                    {
                        _timerDbc.Stop();
                        _timerDbc.Close();
                        _timerDbc = null;
                        lock (invoker)
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


        /// <summary>
        /// 即刻执行，执行之后，在timeMs内再次调用无效
        /// </summary>
        /// <param name="timeMs">不应期，这段时间内调用无效</param>
        /// <param name="invoker">同步对象，一般为控件。 如不需同步可传null</param>
        public void Throttle(int timeMs, ISynchronizeInvoke invoker, Action action)
        {
            System.Threading.Monitor.Enter(this);
            bool needExit = true;
            try
            {
                if (_timerTrt == null)
                {
                    _timerTrt = new System.Timers.Timer(timeMs);
                    _timerTrt.AutoReset = false;
                    _timerTrt.Elapsed += (o, e) =>
                    {
                        _timerTrt.Stop();
                        _timerTrt.Close();
                        _timerTrt = null;
                    };
                    _timerTrt.Start();
                    System.Threading.Monitor.Exit(this);
                    needExit = false;
                    InvokeAction(action, invoker); //这个过程不能锁
                }
            }
            finally
            {
                if (needExit)
                    System.Threading.Monitor.Exit(this);
            }
        }


        private static void InvokeAction(Action action, ISynchronizeInvoke invoker)
        {
            if (invoker == null)
            {
                action.Invoke();
            }
            else
            {
                if (invoker.InvokeRequired)
                {
                    invoker.Invoke(action, null);
                }
                else
                {
                    action();
                }
            }
        }
    }
}