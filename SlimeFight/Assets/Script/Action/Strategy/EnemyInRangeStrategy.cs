#nullable enable
public class EnemyInRangeStrategy : MouseTargetSelectStrategy
{
    readonly float range;

    public override float Range => range;

    public EnemyInRangeStrategy(float range)
    {
        this.range = range;
    }

    protected override bool IsHexValid(HexCoord hex)
    {
        if (!mapManager.TryGetOccupant(hex, out var targetRunTimeId))
            return false;
        if (targetRunTimeId == characterRunTimeId)
            return false;
        if (!characterManager.IsValidAttackTarget(characterRunTimeId, targetRunTimeId))
            return false;
        return characterManager.IsWithinRange(characterRunTimeId, targetRunTimeId, Range);
    }

    protected override void UpdateTargetDisplay(HexCoord hex)
    {
        mapManager.ShowHover(hex, IsHexValid(hex));
    }
}
