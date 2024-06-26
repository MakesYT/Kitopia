﻿using System;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Kitopia.SDKs;

public class TimerHelper
{
    Action action;
    bool lastUp = true;
    Timer timer; // 设置定时时间为1秒
    bool timerActive;

    public TimerHelper(int interval, Action action)
    {
        timer = new Timer(interval);
        this.action = action;
    }

// 当计时器触发时执行的操作
    private void OnTimerElapsed(object source, ElapsedEventArgs e)
    {
        // 如果计时器再次被触发，则重新设置计时器
        if (timerActive)
        {
            timer.Stop();
            timerActive = false;
            if (lastUp)
            {
                ThreadPool.QueueUserWorkItem((e) =>
                {
                    action.Invoke();
                });
            }

            lastUp = false;
        }
        else
        {
            timerActive = true;
            timer.Start();
        }
    }

// 在需要开始计时器的地方调用该方法
    public void StartTimer()
    {
        if (!timerActive)
        {
            timer.Elapsed += OnTimerElapsed;
            timer.Start();
            timerActive = true;
        }
    }

// 在需要停止计时器的地方调用该方法
    public void StopTimer()
    {
        lastUp = true;
        if (timerActive)
        {
            timer.Elapsed -= OnTimerElapsed;
            timer.Stop();
            timerActive = false;
        }
    }
}