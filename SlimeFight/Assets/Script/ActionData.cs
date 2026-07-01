#nullable enable

public class ActionData
{
    public string Id { get; }
    public int ManaCost { get; private set; }
    public ITargetSelectStrategy TargetStrategy { get; private set; } = null!;
    public IActionExecution Execution { get; private set; } = null!;

    public ActionData(string id)
    {
        Id = id;
    }

    public ActionData SetCost(int cost)
    {
        ManaCost = cost;
        return this;
    }

    public ActionData SetTargetedStrategy(ITargetSelectStrategy strategy)
    {
        TargetStrategy = strategy;
        return this;
    }

    public ActionData SetActionExecution(IActionExecution execution)
    {
        Execution = execution;
        return this;
    }
}
