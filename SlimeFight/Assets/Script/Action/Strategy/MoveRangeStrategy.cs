#nullable enable
using UnityEngine;

public class MoveRangeStrategy : MouseTargetSelectStrategy
{
    readonly ActionRangeType rangeType;
    public override ActionRangeType RangeType => rangeType;
    public override float Range => ActionLibrary.GetRange(rangeType);

    public MoveRangeStrategy(ActionRangeType rangeType)
    {
        this.rangeType = rangeType;
    }

    public override bool TryGetTarget(Vector3 mousePosition, out ActionTarget target)
    {
        target = default;
        if (!MapManager.IsPositionOnMap(mousePosition)) return false;
        if (!CharacterManager.IsWithinRange(ActiveCharacterRunTimeId, mousePosition, Range))
            return false;

        target = new ActionTarget(mousePosition);
        return true;
    }
}