#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveAction : CharacterAction
{
    public override string ActionName => "Move";
    Vector2 targetPosition;

    public MoveAction(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
        : base(characterManager, mapManager, activeCharacterRunTimeId) { }

    public override CharacterActionType ActionType => CharacterActionType.Move;

    protected override void OnReset() => targetPosition = default;

    public override bool TrySelectTarget(Vector2 mousePosition)
    {
        if (!MapManager.IsPositionOnMap(mousePosition)) return false;

        targetPosition = mousePosition;
        HasSelectedTarget = true;
        return true;
    }

    public override UniTask ExecuteAsync()
        => CharacterManager.CharacterMoveToPosition(ActiveCharacterRunTimeId, targetPosition);
}
