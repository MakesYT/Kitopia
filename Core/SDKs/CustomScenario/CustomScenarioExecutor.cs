using Core.SDKs.Services.Config;

namespace Core.SDKs.CustomScenario;

public static class CustomScenarioExecutor
{
    public static List<string> OnStartup = new();
    public static List<string> OnExit = new();
    public static List<string> OnSystemClose = new();

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