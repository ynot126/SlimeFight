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
        if (!characterManager.IsWithinRange(characterRunTimeId, position, Range)) return false;
        return !characterManager.IsMovementPathBlocked(characterRunTimeId, position);
    }

    protected override void UpdateTargetDisplay(Vector3 mousePosition)
    {
        var isValid = IsPositionValid(mousePosition);
        characterActionDisplay.SetPosition(mousePosition);
        characterActionDisplay.SetValidTargetVisual(isValid);
        characterActionDisplay.SetVisible(true);
        if (!characterManager.TryGetCharacter(characterRunTimeId, out var character))
        {
            characterActionDisplay.SetMovePathIndicatorVisible(false);
            return;
        }

        characterActionDisplay.SetMovePathIndicator(character.Position, mousePosition, isValid, true);
    }
}
