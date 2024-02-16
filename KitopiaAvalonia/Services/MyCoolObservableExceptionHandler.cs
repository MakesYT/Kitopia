using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using KitopiaAvalonia.Windows;
using log4net;
using ReactiveUI;

namespace KitopiaAvalonia.Services;

public class MyCoolObservableExceptionHandler : IObserver<Exception>
{
    private static readonly ILog Log = LogManager.GetLogger(nameof(MyCoolObservableExceptionHandler));

    public void OnNext(Exception value)
    {
        Program.cts.Cancel();
        Program.cts.Dispose();
        if (Debugger.IsAttached) Debugger.Break();
        Log.Error(value);
        new ErrorDialog(null, value.ToString()).Show();
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            throw value;
        });
    }

    public void OnError(Exception error)
    {
        Program.cts.Cancel();
        Program.cts.Dispose();
        if (Debugger.IsAttached) Debugger.Break();
        Log.Error(error);
        new ErrorDialog(null, error.ToString()).Show();
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            throw error;
        });
    }

    public void OnCompleted()
    {
        Program.cts.Cancel();
        Program.cts.Dispose();
        if (Debugger.IsAttached) Debugger.Break();
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            throw new NotImplementedException();
        });
    }
}