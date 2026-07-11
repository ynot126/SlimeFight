#nullable enable
using UnityEngine;

public class EnemyInRangeStrategy : MouseTargetSelectStrategy
{
    readonly ActionRangeType rangeType;

    public override ActionRangeType RangeType => rangeType;
    public override float Range => ActionLibrary.GetRange(rangeType);

    public EnemyInRangeStrategy(ActionRangeType rangeType)
    {
        this.rangeType = rangeType;
    }

    protected override bool IsHexValid(HexCoord hex)
    {
        if (!mapManager.TryGetOccupant(hex, out var targetRunTimeId))
            return false;
        if (targetRunTimeId == characterRunTimeId)
            return false;
        if (!characterManager.IsValidAttackTarget(characterRunTimeId, targetRunTimeId))
            return false;
        return characterManager.IsWithinRange(characterRunTimeId, targetRunTimeId, Range);
    }

    protected override void UpdateTargetDisplay(HexCoord hex, Vector3 snappedPosition)
    {
        characterActionDisplay.SetPosition(snappedPosition);
        characterActionDisplay.SetValidTargetVisual(IsHexValid(hex));
        characterActionDisplay.SetVisible(true);
    }
}
