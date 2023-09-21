using Core.SDKs.Services.Config;

namespace Core.SDKs.CustomScenario;

public enum CustomScenarioType
{
    Startup,
    Exit,
    SystemClose
}

public static class CustomScenarioExecutor
{
    private static List<string> OnStartup = new();
    private static List<string> OnExit = new();
    private static List<string> OnSystemClose = new();

    public static void AddAutoCustomScenario(string scenarioName, CustomScenarioType scenarioType)
    {
        switch (scenarioType)
        {
            case CustomScenarioType.Startup:
                OnStartup.Add(scenarioName);
                break;
            case CustomScenarioType.Exit:
                OnExit.Add(scenarioName);
                break;
            case CustomScenarioType.SystemClose:
                OnSystemClose.Add(scenarioName);
                break;
        }
    }

    public static void Startup()
    {
        foreach (var s in OnStartup)
        {
            foreach (var customScenario in CustomScenarioManger.CustomScenarios.Where((e) => e.UUID.Equals(s)))
            {
                customScenario.Run();
            }
        }
    }

    public static void SystemClose()
    {
        foreach (var s in OnSystemClose)
        {
            foreach (var customScenario in CustomScenarioManger.CustomScenarios.Where((e) => e.UUID.Equals(s)))
            {
                customScenario.Run();
            }
        }
    }

    public static void Exit()
    {
        foreach (var s in OnExit)
        {
            foreach (var customScenario in CustomScenarioManger.CustomScenarios.Where((e) => e.UUID.Equals(s)))
            {
                customScenario.Run();
            }
        }
    }
}