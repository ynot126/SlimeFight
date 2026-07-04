#nullable enable

public abstract class AutoTargetSelectStrategy : BaseTargetSelectStrategy
{
    public sealed override bool TryGetTarget(out ActionTarget target) => TryAutoSelect(out target);

    protected abstract bool TryAutoSelect(out ActionTarget target);
}
