namespace Core.SDKs.CustomScenario;

public class CustomScenarioExecutor
{
    private List<CustomScenario> scenarios = new();

    public void AddCustomScenario(CustomScenario scenario)
    {
        scenarios.Add(scenario);
    }

    public void Execute()
    {
        foreach (var scenario in scenarios)
        {
            scenario.Run();
        }
    }

    public void DelCustomScenario(CustomScenario scenario)
    {
        scenarios.Remove(scenario);
    }
}