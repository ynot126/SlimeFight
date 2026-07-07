using System.Collections.Generic;

public class EnemyData
{
    readonly string id;
    public EntityStat stat;
    public List<string> actionIds = new();
    public BotPlannerData BotPlannerData { get; private set; } = new();

    public EnemyData(string aId)
    {
        id = aId;
    }

    public EnemyData SetStat(EntityStat aStat)
    {
        stat = aStat;
        return this;
    }

    public EnemyData SetActionIds(params string[] aActionIds)
    {
        actionIds.Clear();
        actionIds.AddRange(aActionIds);
        return this;
    }

    public EnemyData SetBotPlannerData(BotPlannerData aBotPlannerData)
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