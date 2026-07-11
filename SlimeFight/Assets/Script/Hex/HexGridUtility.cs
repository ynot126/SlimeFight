#nullable enable
using System.Collections.Generic;
using UnityEngine;

public static class HexGridUtility
{
    static readonly HexCoord[] directions =
    {
        new(1, 0),
        new(1, -1),
        new(0, -1),
        new(-1, 0),
        new(-1, 1),
        new(0, 1),
    };

    public static IReadOnlyList<HexCoord> Directions => directions;

    public static Vector3 HexToWorld(HexCoord hex, float hexSize)
    {
        var x = hexSize * Mathf.Sqrt(3f) * (hex.Q + hex.R * 0.5f);
        var z = hexSize * 1.5f * hex.R;
        return new Vector3(x, 0f, z);
    }

    public static HexCoord WorldToHex(Vector3 position, float hexSize)
    {
        var q = (Mathf.Sqrt(3f) / 3f * position.x - 1f / 3f * position.z) / hexSize;
        var r = (2f / 3f * position.z) / hexSize;
        return RoundAxial(q, r);
    }

    public static int Distance(HexCoord a, HexCoord b)
        => Mathf.Max(Mathf.Max(Mathf.Abs(a.Q - b.Q), Mathf.Abs(a.R - b.R)), Mathf.Abs(a.S - b.S));

    public static List<HexCoord> GetHexesInRange(HexCoord center, int range)
    {
        var results = new List<HexCoord>();
        for (var q = -range; q <= range; q++)
        {
            var minR = Mathf.Max(-range, -q - range);
            var maxR = Mathf.Min(range, -q + range);
            for (var r = minR; r <= maxR; r++)
                results.Add(new HexCoord(center.Q + q, center.R + r));
        }

        return results;
    }

    public static Vector3[] GetCornerPositions(Vector3 center, float hexSize, float y)
    {
        var corners = new Vector3[6];
        for (var i = 0; i < corners.Length; i++)
        {
            var angle = Mathf.Deg2Rad * (60f * i - 30f);
            corners[i] = new Vector3(
                center.x + hexSize * Mathf.Cos(angle),
                y,
                center.z + hexSize * Mathf.Sin(angle));
        }

        return corners;
    }

    public static Vector3[] GetClosedCornerPositions(Vector3 center, float hexSize, float y)
    {
        var corners = GetCornerPositions(center, hexSize, y);
        var closedCorners = new Vector3[7];
        for (var i = 0; i < corners.Length; i++)
            closedCorners[i] = corners[i];
        closedCorners[closedCorners.Length - 1] = corners[0];
        return closedCorners;
    }

    static HexCoord RoundAxial(float q, float r)
    {
        var s = -q - r;
        var roundedQ = Mathf.RoundToInt(q);
        var roundedR = Mathf.RoundToInt(r);
        var roundedS = Mathf.RoundToInt(s);

        var qDiff = Mathf.Abs(roundedQ - q);
        var rDiff = Mathf.Abs(roundedR - r);
        var sDiff = Mathf.Abs(roundedS - s);

        if (qDiff > rDiff && qDiff > sDiff)
            roundedQ = -roundedR - roundedS;
        else if (rDiff > sDiff)
            roundedR = -roundedQ - roundedS;

        return new HexCoord(roundedQ, roundedR);
    }
}
