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

    public override bool TryGetTarget(Vector3 mousePosition, out ActionTarget target)
    {
        target = default;
        if (!CharacterManager.TryGetCharacterAtPosition(mousePosition, ActiveCharacterRunTimeId, out var character))
            return false;
        if (!CharacterManager.IsValidAttackTarget(ActiveCharacterRunTimeId, character.RunTimeId))
            return false;
        if (!CharacterManager.IsWithinRange(ActiveCharacterRunTimeId, character.RunTimeId, Range))
            return false;

        target = new ActionTarget(character.Position, character.RunTimeId);
        return true;
    }
}