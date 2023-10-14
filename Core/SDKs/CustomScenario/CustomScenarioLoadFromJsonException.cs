namespace Core.SDKs.CustomScenario;

public class CustomScenarioLoadFromJsonException : Exception
{
    public string PluginName
    {
        get;
        set;
    }

    public string MethodName
    {
        get;
        set;
    }

    public CustomScenarioLoadFromJsonException(string pluginName, string methodName)
    {
        this.PluginName = pluginName;
        this.MethodName = methodName;
    }
}