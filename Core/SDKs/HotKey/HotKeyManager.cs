using Core.SDKs.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Core.SDKs.HotKey;

public class HotKeyManager
{
    public static IHotKetImpl HotKetImpl;

    public static void Init()
    {
        HotKetImpl = ServiceManager.Services.GetService<IHotKetImpl>()!;
        HotKetImpl.Init();
    }
}