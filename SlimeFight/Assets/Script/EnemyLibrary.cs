#nullable enable
using System;
using System.Collections.Generic;

public static class EnemyLibrary
{
    static readonly Dictionary<string, Func<EnemyData>> enemyFactory = new Dictionary<string, Func<EnemyData>>();
    static readonly Dictionary<string, EnemyData> enemyCache = new Dictionary<string, EnemyData>();

    static EnemyLibrary()
    {
        enemyFactory["Testing"] = () => new EnemyData("Testing")
            .SetStat(new EntityStat(5, 2, 1, 2, 0, 2))
            .SetActionIds("strike", "move")
            .SetBotPlannerData(new BotPlannerData(2));
    }
    public static EnemyData GetEnemy(string actionId)
    {
        if (enemyCache.TryGetValue(actionId, out var cached)) return cached;
        var enemy = enemyFactory[actionId].Invoke();
        enemyCache[actionId] = enemy;
        return enemy;
    }

    public static List<EnemyData> GetEnemy(List<string> enemyIds)
    {
        var enemy = new List<EnemyData>();
        foreach (var id in enemyIds) enemy.Add(GetEnemy(id));
        return enemy;
    }
}
