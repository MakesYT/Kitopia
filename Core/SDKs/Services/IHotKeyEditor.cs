using Core.SDKs.HotKey;

namespace Core.SDKs.Services;

public interface IHotKeyEditor
{
    void EditByName(string name, object? owner);
    void EditByHotKeyModel(HotKeyModel name, object? owner);
    void UnuseByHotKeyModel(HotKeyModel hotKeyModel);
    void RemoveByHotKeyModel(HotKeyModel hotKeyModel);
}