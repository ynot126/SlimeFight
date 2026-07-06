#nullable enable
using UnityEngine;

public abstract class MouseTargetSelectStrategy : BaseTargetSelectStrategy
{
    CharacterActionDisplay targetDisplay = null!;

    public void Initialize(ActionContext ctx, CharacterActionDisplay display)
    {
        base.Initialize(ctx);
        targetDisplay = display;
    }

    public sealed override bool TryGetTarget(out ActionTarget target)
    {
        target = default;
        return false;
    }

    public abstract bool TryGetTarget(Vector3 mousePosition, out ActionTarget target);

    public void UpdateTargetDisplay(Vector3 mousePosition)
    {
        targetDisplay.SetPosition(mousePosition);
        targetDisplay.SetValidTargetVisual(TryGetTarget(mousePosition, out _));
        targetDisplay.SetVisible(true);
    }

    public override void HideTargetDisplay()
    {
        targetDisplay.SetVisible(false);
    }
}
