using Core.SDKs.HotKey;

namespace Core.SDKs.Services;

public interface IHotKetImpl
{
    public void Init();
    public bool Add(HotKeyModel hotKeyModel, Action<HotKeyModel> rallBack);
    public bool Del(HotKeyModel hotKeyModel);
    public bool Del(string uuid);
    public bool RequestUserModify(string uuid);
    public bool Modify(HotKeyModel hotKeyModel);
    public HotKeyModel? GetByUuid(string uuid);
    public IEnumerable<HotKeyModel> GetAllRegistered();

    public IEnumerable<HotKeyModel> AllRegistered
    {
        get => GetAllRegistered();
    }
}