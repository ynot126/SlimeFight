#nullable enable
using UnityEngine;

public class MoveRangeStrategy : ITargetSelectStrategy
{
    readonly ActionRangeType rangeType;
    public ActionRangeType RangeType => rangeType;
    public float Range => ActionLibrary.GetRange(rangeType);

    public MoveRangeStrategy(ActionRangeType rangeType)
    {
        this.rangeType = rangeType;
    }

    public bool TrySelectTarget(ActionContext ctx, Vector3 mousePosition, out ActionTarget target)
    {
        target = default;
        if (!ctx.MapManager.IsPositionOnMap(mousePosition)) return false;
        if (!ctx.CharacterManager.IsWithinRange(ctx.ActiveCharacterRunTimeId, mousePosition, Range))
            return false;

        target = new ActionTarget(mousePosition);
        return true;
    }
}