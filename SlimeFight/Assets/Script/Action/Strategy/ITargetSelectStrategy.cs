#nullable enable
using UnityEngine;

public interface ITargetSelectStrategy
{
    float Range { get; }
    bool TrySelectTarget(ActionContext ctx, Vector2 mousePosition, out ActionTarget target);
}
