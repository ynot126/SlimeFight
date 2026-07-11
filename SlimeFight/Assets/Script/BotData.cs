#nullable enable
using System.Collections.Generic;

public class BotData
{
    readonly string id;
    public CharacterType characterType = CharacterType.Enemy;
    public EntityStat stat = null!;
    public List<string> actionIds = new();
    public BotPlannerData BotPlannerData { get; private set; } = new();

    public BotData(string aId)
    {
        id = aId;
    }

    public BotData SetStat(EntityStat aStat)
    {
        stat = aStat;
        return this;
    }

    public BotData SetActionIds(params string[] aActionIds)
    {
        actionIds.Clear();
        actionIds.AddRange(aActionIds);
        return this;
    }

    public BotData SetBotPlannerData(BotPlannerData aBotPlannerData)
    {
        BotPlannerData = aBotPlannerData;
        return this;
    }
}

public class BotPlannerData
{
    public int MaxActionsPerTurn { get; }

    public BotPlannerData(int maxActionsPerTurn = 2)
    {
        MaxActionsPerTurn = maxActionsPerTurn;
    }
}