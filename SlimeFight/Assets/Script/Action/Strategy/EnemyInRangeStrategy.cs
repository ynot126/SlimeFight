#nullable enable
using UnityEngine;

public class EnemyInRangeStrategy : ITargetSelectStrategy
{
    readonly float range;
    public float Range => range;

    public EnemyInRangeStrategy(float range)
    {
        this.range = range;
    }

    public bool TrySelectTarget(ActionContext ctx, Vector2 mousePosition, out ActionTarget target)
    {
        target = default;
        if (!ctx.CharacterManager.TryGetCharacterAtPosition(mousePosition, ctx.ActiveCharacterRunTimeId, out var character))
            return false;
        if (!ctx.CharacterManager.IsValidAttackTarget(ctx.ActiveCharacterRunTimeId, character.RunTimeId))
            return false;
        if (!ctx.CharacterManager.IsWithinRange(ctx.ActiveCharacterRunTimeId, character.RunTimeId, range))
            return false;

        target = new ActionTarget(mousePosition, character.RunTimeId);
        return true;
    }
}
