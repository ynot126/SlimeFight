using System;
using System.Collections.Generic;

public static class ActionLibrary
{
    static EnumDictionary<ActionRangeType, int> actionRanges = new EnumDictionary<ActionRangeType, int>()
    {
        [ActionRangeType.None] = 0,
        [ActionRangeType.Melee] = 2,
        [ActionRangeType.Ranged] = 5,
        [ActionRangeType.Move] = 3,
        [ActionRangeType.World] = 100,
    };

    public static float GetRange(ActionRangeType rangeType) => actionRanges[rangeType];
    static readonly Dictionary<string, Func<ActionData>> actionFactory = new Dictionary<string, Func<ActionData>>();
    static readonly Dictionary<string, ActionData> actionCache = new Dictionary<string, ActionData>();

    static ActionLibrary()
    {
        actionFactory["strike"] = () => new ActionData("Strike")
            .SetCost(1)
            .SetTargetedStrategy(new EnemyInRangeStrategy(ActionRangeType.Melee))
            .SetActionExecution(new AttackEffect(5));

        actionFactory["move"] = () => new ActionData("Move")
            .SetCost(1)
            .SetTargetedStrategy(new MoveRangeStrategy(ActionRangeType.Move))
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
