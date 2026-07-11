using System;
using System.Collections.Generic;

public static class ActionLibrary
{
    static readonly Dictionary<string, Func<ActionData>> actionFactory = new Dictionary<string, Func<ActionData>>();
    static readonly Dictionary<string, ActionData> actionCache = new Dictionary<string, ActionData>();

    static ActionLibrary()
    {
        actionFactory["strike"] = () => new ActionData("Strike")
            .SetCost(1)
            .SetTargetedStrategy(new EnemyInRangeStrategy(2))
            .SetActionExecution(new AttackEffect(5));

        actionFactory["move"] = () => new ActionData("Move")
            .SetCost(1)
            .SetTargetedStrategy(new MoveRangeStrategy(3))
            .SetActionExecution(new MoveEffect());
    }

    public static ActionData GetAction(string actionId)
    {
        if (actionCache.TryGetValue(actionId, out var cached)) return cached;
        var action = actionFactory[actionId].Invoke();
        actionCache[actionId] = action;
        return action;
    }

    public static List<ActionData> GetAction(List<string> actionIds)
    {
        var actions = new List<ActionData>();
        foreach (var id in actionIds) actions.Add(GetAction(id));
        return actions;
    }
}
