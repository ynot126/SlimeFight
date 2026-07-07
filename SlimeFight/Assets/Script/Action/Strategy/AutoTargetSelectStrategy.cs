#nullable enable

using System;
using System.Collections.Generic;

public abstract class AutoTargetSelectStrategy : BaseTargetSelectStrategy
{
    public List<ActionTarget>? GetTarget()
        => throw new NotImplementedException("AutoTargetSelectStrategy is not implemented yet.");
}
