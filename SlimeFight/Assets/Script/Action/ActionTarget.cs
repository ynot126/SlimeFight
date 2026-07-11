#nullable enable
using UnityEngine;

public readonly struct ActionTarget
{
    readonly Vector3 position;
    readonly HexCoord hex;
    readonly bool hasHex;
    readonly int targetCharacterRunTimeId;

    public Vector3 Position => position;
    public HexCoord Hex => hex;
    public bool HasHex => hasHex;
    public int TargetCharacterRunTimeId => targetCharacterRunTimeId;

    public ActionTarget(Vector3 position, int targetCharacterRunTimeId = 0)
    {
        this.position = position;
        hex = default;
        hasHex = false;
        this.targetCharacterRunTimeId = targetCharacterRunTimeId;
    }

    public ActionTarget(HexCoord hex, Vector3 position, int targetCharacterRunTimeId = 0)
    {
        this.hex = hex;
        this.position = position;
        hasHex = true;
        this.targetCharacterRunTimeId = targetCharacterRunTimeId;
    }
}
