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

    protected override bool IsPositionValid(Vector3 position)
    {
        if (!mapManager.IsPositionOnMap(position)) return false;
        return characterManager.IsWithinRange(characterRunTimeId, position, Range);
    }

    protected override void UpdateTargetDisplay(Vector3 mousePosition)
    {
        characterActionDisplay.SetPosition(mousePosition);
        characterActionDisplay.SetValidTargetVisual(IsPositionValid(mousePosition));
        characterActionDisplay.SetVisible(true);
    }
}
