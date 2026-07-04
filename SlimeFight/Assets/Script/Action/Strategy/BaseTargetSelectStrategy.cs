#nullable enable
using UnityEngine;

public abstract class BaseTargetSelectStrategy
{
    ActionContext context;
    protected CharacterManager CharacterManager => context.CharacterManager;
    protected MapManager MapManager => context.MapManager;
    protected int ActiveCharacterRunTimeId => context.ActiveCharacterRunTimeId;
    public void Initialize(ActionContext ctx) => context = ctx;

    public abstract ActionRangeType RangeType { get; }
    public abstract float Range { get; }
    public abstract bool TryGetTarget(out ActionTarget target);
}

public enum ActionRangeType
{
    None,
    Melee,
    Ranged,
    Move,
    World,
}
