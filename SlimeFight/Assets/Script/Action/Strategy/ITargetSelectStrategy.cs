#nullable enable
using UnityEngine;

public interface ITargetSelectStrategy
{
    float Range { get; }
    bool TrySelectTarget(ActionContext ctx, Vector3 mousePosition, out ActionTarget target);
}
