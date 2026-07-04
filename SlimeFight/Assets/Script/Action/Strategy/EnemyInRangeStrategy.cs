#nullable enable
using UnityEngine;

public class EnemyInRangeStrategy : ITargetSelectStrategy
{
    readonly ActionRangeType rangeType;
    public ActionRangeType RangeType => rangeType;
    public float Range => ActionLibrary.GetRange(rangeType);

    public EnemyInRangeStrategy(ActionRangeType rangeType)
    {
        this.rangeType = rangeType;
    }

    public bool TrySelectTarget(ActionContext ctx, Vector3 mousePosition, out ActionTarget target)
    {
        target = default;
        if (!ctx.CharacterManager.TryGetCharacterAtPosition(mousePosition, ctx.ActiveCharacterRunTimeId, out var character))
            return false;
        if (!ctx.CharacterManager.IsValidAttackTarget(ctx.ActiveCharacterRunTimeId, character.RunTimeId))
            return false;
        if (!ctx.CharacterManager.IsWithinRange(ctx.ActiveCharacterRunTimeId, character.RunTimeId, Range))
            return false;

        target = new ActionTarget(character.Position, character.RunTimeId);
        return true;
    }
}
