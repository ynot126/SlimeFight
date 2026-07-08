#nullable enable
using System;
using System.Collections.Generic;

public static class BotLibrary
{
    static readonly Dictionary<string, Func<BotData>> botFactory = new Dictionary<string, Func<BotData>>();
    static readonly Dictionary<string, BotData> botCache = new Dictionary<string, BotData>();

    static BotLibrary()
    {
        botFactory["Testing"] = () => new BotData("Testing")
            .SetStat(new EntityStat(5, 2, 1, 2, 0, 2))
            .SetActionIds("strike", "move")
            .SetBotPlannerData(new BotPlannerData(2));
    }

    public static BotData GetBot(string actionId)
    {
        if (botCache.TryGetValue(actionId, out var cached)) return cached;
        var bot = botFactory[actionId].Invoke();
        botCache[actionId] = bot;
        return bot;
    }

    public static List<BotData> GetBot(List<string> botIds)
    {
        var bot = new List<BotData>();
        foreach (var id in botIds) bot.Add(GetBot(id));
        return bot;
    }
}
