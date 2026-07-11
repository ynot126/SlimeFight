#nullable enable
public class MoveRangeStrategy : MouseTargetSelectStrategy
{
    readonly ActionRangeType rangeType;

    public override ActionRangeType RangeType => rangeType;
    public override float Range => ActionLibrary.GetRange(rangeType);

    public MoveRangeStrategy(ActionRangeType rangeType)
    {
        this.rangeType = rangeType;
    }

    protected override bool IsHexValid(HexCoord hex)
    {
        if (!mapManager.IsHexOnMap(hex)) return false;
        var position = mapManager.HexToWorld(hex);
        if (!characterManager.IsWithinRange(characterRunTimeId, position, Range)) return false;
        return !characterManager.IsMovementPathBlocked(characterRunTimeId, position);
    }

    protected override void UpdateTargetDisplay(HexCoord hex)
    {
        mapManager.ShowHover(hex, IsHexValid(hex));
    }
}
