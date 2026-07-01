#nullable enable
using UnityEngine;

public readonly struct ActionTarget
{
    readonly Vector2 position;
    readonly int targetCharacterRunTimeId;

    public Vector2 Position => position;
    public int TargetCharacterRunTimeId => targetCharacterRunTimeId;

    public ActionTarget(Vector2 position, int targetCharacterRunTimeId = 0)
    {
        this.position = position;
        this.targetCharacterRunTimeId = targetCharacterRunTimeId;
    }
}
