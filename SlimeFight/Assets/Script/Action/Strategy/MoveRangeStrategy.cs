#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class MoveRangeStrategy : MouseTargetSelectStrategy
{
    readonly float range;
    readonly HashSet<HexCoord> reachableHexes = new();

    public override float Range => range;

    public MoveRangeStrategy(float range)
    {
        this.range = range;
    }

    protected override bool IsHexValid(HexCoord hex)
    {
        return mapManager.IsHexOnMap(hex) && reachableHexes.Contains(hex);
    }

    public override void ShowTargetPreview()
    {
        reachableHexes.Clear();
        var reachable = characterManager.GetReachableHexes(
            characterRunTimeId,
            Mathf.RoundToInt(Range));

        foreach (var hex in reachable.Keys)
            reachableHexes.Add(hex);

        mapManager.ShowRange(reachableHexes);
    }

    protected override void UpdateTargetDisplay(HexCoord hex)
    {
        mapManager.ShowHover(hex, IsHexValid(hex));
    }

    protected override void ClearTargetDisplay()
    {
        base.ClearTargetDisplay();
        reachableHexes.Clear();
    }
}
