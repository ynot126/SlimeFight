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

    protected override bool IsHexValid(HexCoord hex)
    {
        if (!mapManager.IsHexOnMap(hex)) return false;
        var position = mapManager.HexToWorld(hex);
        if (!characterManager.IsWithinRange(characterRunTimeId, position, Range)) return false;
        return !characterManager.IsMovementPathBlocked(characterRunTimeId, position);
    }

    protected override void UpdateTargetDisplay(HexCoord hex, Vector3 snappedPosition)
    {
        var isValid = IsHexValid(hex);
        characterActionDisplay.SetPosition(snappedPosition);
        characterActionDisplay.SetValidTargetVisual(isValid);
        characterActionDisplay.SetVisible(true);
        if (!characterManager.TryGetCharacter(characterRunTimeId, out var character))
        {
            characterActionDisplay.SetMovePathIndicatorVisible(false);
            return;
        }

        if (characterManager.TryGetPathToPosition(characterRunTimeId, snappedPosition, out var pathPositions))
            characterActionDisplay.SetMovePathIndicator(pathPositions, isValid, true);
        else
            characterActionDisplay.SetMovePathIndicator(character.Position, snappedPosition, isValid, true);
    }
}
