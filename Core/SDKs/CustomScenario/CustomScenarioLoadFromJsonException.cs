namespace Core.SDKs.CustomScenario;

public class CustomScenarioLoadFromJsonException : Exception
{
    public string PluginName;
    public string MethodName;

    public CustomScenarioLoadFromJsonException(string PluginName, string MethodName)
    {
        this.PluginName = PluginName;
        this.MethodName = MethodName;
    }
}