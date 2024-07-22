using Core.SDKs.HotKey;

namespace Core.SDKs.Services;

public interface IHotKetImpl
{
    public void Init();
    public bool Add(HotKeyModel hotKeyModel, Action<HotKeyModel> rallBack);
    public bool Del(HotKeyModel hotKeyModel);
    public bool Del(string uuid);
}