#nullable enable
using System;
using System.Collections.Generic;

public static class HexPathfinder
{
    public static Dictionary<HexCoord, int> FindReachable(
        HexCoord start,
        int maxSteps,
        ICollection<HexCoord> validHexes,
        Func<HexCoord, bool> canEnter)
    {
        var distances = new Dictionary<HexCoord, int> { [start] = 0 };
        var frontier = new Queue<HexCoord>();
        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            var nextDistance = distances[current] + 1;
            if (nextDistance > maxSteps) continue;

            foreach (var direction in HexGridUtility.Directions)
            {
                var next = current + direction;
                if (!validHexes.Contains(next)) continue;
                if (distances.ContainsKey(next)) continue;
                if (!canEnter(next)) continue;

                distances[next] = nextDistance;
                frontier.Enqueue(next);
            }
        }

        return distances;
    }

    public static bool TryFindPath(
        HexCoord start,
        HexCoord goal,
        ICollection<HexCoord> validHexes,
        Func<HexCoord, bool> canEnter,
        out List<HexCoord> path)
    {
        path = new List<HexCoord>();
        if (!validHexes.Contains(start) || !validHexes.Contains(goal)) return false;
        if (start == goal)
        {
            path.Add(start);
            return true;
        }

        var frontier = new Queue<HexCoord>();
        var cameFrom = new Dictionary<HexCoord, HexCoord>();
        frontier.Enqueue(start);
        cameFrom[start] = start;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            foreach (var direction in HexGridUtility.Directions)
            {
                var next = current + direction;
                if (!validHexes.Contains(next)) continue;
                if (cameFrom.ContainsKey(next)) continue;
                if (next != goal && !canEnter(next)) continue;
                if (next == goal && !canEnter(next)) continue;

                cameFrom[next] = current;
                if (next == goal)
                {
                    BuildPath(start, goal, cameFrom, path);
                    return true;
                }

                frontier.Enqueue(next);
            }
        }

        return false;
    }

    static void BuildPath(
        HexCoord start,
        HexCoord goal,
        IReadOnlyDictionary<HexCoord, HexCoord> cameFrom,
        List<HexCoord> path)
    {
        var current = goal;
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(start);
        path.Reverse();
    }
}
