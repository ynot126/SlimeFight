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

    protected override bool IsPositionValid(Vector3 position)
    {
        if (!characterManager.TryGetCharacterAtPosition(position, characterRunTimeId, out var targetRunTimeId))
            return false;
        if (!characterManager.IsValidAttackTarget(characterRunTimeId, targetRunTimeId))
            return false;
        return characterManager.IsWithinRange(characterRunTimeId, targetRunTimeId, Range);
    }

    protected override void UpdateTargetDisplay(Vector3 mousePosition)
    {
        characterActionDisplay.SetPosition(mousePosition);
        characterActionDisplay.SetValidTargetVisual(IsPositionValid(mousePosition));
        characterActionDisplay.SetVisible(true);
    }
}
