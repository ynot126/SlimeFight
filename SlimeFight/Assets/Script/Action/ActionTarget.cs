#nullable enable
using UnityEngine;

public readonly struct ActionTarget
{
    public Vector2 Position { get; }
    public int TargetCharacterRunTimeId { get; }

    public ActionTarget(Vector2 position, int targetCharacterRunTimeId = 0)
    {
        Position = position;
        TargetCharacterRunTimeId = targetCharacterRunTimeId;
    }
}
