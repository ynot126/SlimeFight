#nullable enable
using UnityEngine;

public abstract class MouseTargetSelectStrategy : BaseTargetSelectStrategy
{
    public sealed override bool TryGetTarget(out ActionTarget target)
    {
        target = default;
        return false;
    }

    public abstract bool TryGetTarget(Vector3 mousePosition, out ActionTarget target);

    public void UpdateTargetDisplay(Vector3 mousePosition)
    {
        var display = CharacterManager.TargetSelectDisplay;
        display.SetPosition(mousePosition);
        display.SetValidTargetVisual(TryGetTarget(mousePosition, out _));
        display.SetVisible(true);
    }

    public override void HideTargetDisplay()
    {
        CharacterManager.TargetSelectDisplay.SetVisible(false);
    }
}
