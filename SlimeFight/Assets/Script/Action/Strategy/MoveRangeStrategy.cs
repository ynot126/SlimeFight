#nullable enable
using UnityEngine;

public class MoveRangeStrategy : ITargetSelectStrategy
{
    readonly int range;
    public float Range => range;

    public MoveRangeStrategy(int range)
    {
        this.range = range;
    }

    public bool TrySelectTarget(ActionContext ctx, Vector2 mousePosition, out ActionTarget target)
    {
        target = default;
        if (!ctx.MapManager.IsPositionOnMap(mousePosition)) return false;
        if (!ctx.CharacterManager.IsWithinRange(ctx.ActiveCharacterRunTimeId, mousePosition, range))
            return false;

        target = new ActionTarget(mousePosition);
        return true;
    }
}
