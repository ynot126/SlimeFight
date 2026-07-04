#nullable enable
using UnityEngine;

public interface ITargetSelectStrategy
{
    ActionRangeType RangeType { get; }
    float Range { get; }
    bool TrySelectTarget(ActionContext ctx, Vector3 mousePosition, out ActionTarget target);
}

public enum ActionRangeType
{
    None,
    Melee,
    Ranged,
    Move,
    World,
}
