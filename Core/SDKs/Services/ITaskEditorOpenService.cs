using Core.SDKs.Services.Config;

namespace Core.SDKs.Services;

public interface ITaskEditorOpenService
{
    void Open();
    void Open(CustomScenario name);
}