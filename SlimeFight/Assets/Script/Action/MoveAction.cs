#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MoveAction : CharacterAction
{
    public override string ActionName => "Move";

    public MoveAction(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
        : base(characterManager, mapManager, activeCharacterRunTimeId) { }

    public override CharacterActionType ActionType => CharacterActionType.Move;
    public override int ManaCost => 1;

    public override bool TrySelectTarget(Vector2 mousePosition)
    {
        if (!MapManager.IsPositionOnMap(mousePosition)) return false;

        TargetPosition = mousePosition;
        HasSelectedTarget = true;
        return true;
    }

    public override UniTask ExecuteAsync()
        => CharacterManager.CharacterMoveToPosition(ActiveCharacterRunTimeId, TargetPosition);
}
