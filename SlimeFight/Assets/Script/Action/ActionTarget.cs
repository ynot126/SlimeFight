#nullable enable
using UnityEngine;

public readonly struct ActionTarget
{
    readonly Vector3 position;
    readonly int targetCharacterRunTimeId;

    public Vector3 Position => position;
    public int TargetCharacterRunTimeId => targetCharacterRunTimeId;

    public ActionTarget(Vector3 position, int targetCharacterRunTimeId = 0)
    {
        this.position = position;
        this.targetCharacterRunTimeId = targetCharacterRunTimeId;
    }
}
