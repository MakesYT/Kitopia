using Core.SDKs.HotKey;

namespace Core.SDKs.Services;

public interface IHotKeyEditor
{
    void EditByName(string name, object? owner);
    void EditByHotKeyModel(HotKeyModel name, object? owner);
}