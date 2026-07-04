#nullable enable

public class ActionData
{
    readonly string id;
    int manaCost;
    BaseTargetSelectStrategy baseTargetStrategy = null!;
    IActionExecution execution = null!;

    public string Id => id;
    public int ManaCost => manaCost;
    public BaseTargetSelectStrategy BaseTargetStrategy => baseTargetStrategy;
    public IActionExecution Execution => execution;

    public ActionData(string id)
    {
        this.id = id;
    }

    public ActionData SetCost(int cost)
    {
        manaCost = cost;
        return this;
    }

    public ActionData SetTargetedStrategy(BaseTargetSelectStrategy strategy)
    {
        baseTargetStrategy = strategy;
        return this;
    }

    public ActionData SetActionExecution(IActionExecution execution)
    {
        this.execution = execution;
        return this;
    }
}
