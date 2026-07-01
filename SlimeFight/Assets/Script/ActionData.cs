#nullable enable

public class ActionData
{
    readonly string id;
    int manaCost;
    ITargetSelectStrategy targetStrategy = null!;
    IActionExecution execution = null!;

    public string Id => id;
    public int ManaCost => manaCost;
    public ITargetSelectStrategy TargetStrategy => targetStrategy;
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

    public ActionData SetTargetedStrategy(ITargetSelectStrategy strategy)
    {
        targetStrategy = strategy;
        return this;
    }

    public ActionData SetActionExecution(IActionExecution execution)
    {
        this.execution = execution;
        return this;
    }
}
