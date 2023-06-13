namespace PluginCore;

public interface IPlugin
{
    public PluginInfo PluginInfo();
    public void OnEnabled();
    public void OnDisabled();
}