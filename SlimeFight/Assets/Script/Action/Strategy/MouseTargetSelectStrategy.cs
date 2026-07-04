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
}
