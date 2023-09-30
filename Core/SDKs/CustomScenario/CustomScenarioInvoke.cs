namespace Core.SDKs.CustomScenario;

public enum TriggerType
{
    手动,
    自动,
    定时,
    定周期
}

public enum AutoTriggerType
{
    软件启动时,
    软件关闭时,
    系统关闭时,
}

public class CustomScenarioInvoke
{
    private CustomScenario _customScenario;

    public TriggerType TriggerType
    {
        get;
        set;
    }

    public AutoTriggerType AutoTriggerType
    {
        get;
        set;
    }

    public object? Value
    {
        get;
        set;
    }
}