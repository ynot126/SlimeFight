#nullable enable
using UnityEngine;

public interface ITargetSelectStrategy
{
    bool TrySelectTarget(ActionContext ctx, Vector2 mousePosition, out ActionTarget target);
}
