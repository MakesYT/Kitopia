namespace Core.SDKs.CustomScenario;

public static class CustomScenarioExecutorManager
{
    public static CustomScenarioExecutor SoftwareStarted = new();
    public static CustomScenarioExecutor SoftwareShutdown = new();
    public static CustomScenarioExecutor SystemShutdown = new();
    public static Dictionary<string, CustomScenarioExecutor> CustomExecutors = new();
    public static Dictionary<string, string> AllExecutors = new();

    public static void Initial()
    {
        AllExecutors.Add("软件启动时", "");
    }
}