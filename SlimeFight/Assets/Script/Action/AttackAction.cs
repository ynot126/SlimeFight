#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttackAction : CharacterAction
{
    public override string ActionName => "Attack";

    public AttackAction(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
        : base(characterManager, mapManager, activeCharacterRunTimeId) { }

    public override CharacterActionType ActionType => CharacterActionType.Attack;
    public override int ManaCost => 2;

    public override bool TrySelectTarget(Vector2 mousePosition)
    {
        if (!CharacterManager.TryGetCharacterAtPosition(mousePosition, ActiveCharacterRunTimeId, out var target))
            return false;
        if (!CharacterManager.IsValidAttackTarget(ActiveCharacterRunTimeId, target.RunTimeId))
            return false;

        TargetPosition = mousePosition;
        HasSelectedTarget = true;
        return true;
    }

    public override async UniTask ExecuteAsync()
    {
        if (!CharacterManager.TryGetCharacterAtPosition(TargetPosition, ActiveCharacterRunTimeId, out var target))
            return;
        CharacterManager.CharacterAttack(ActiveCharacterRunTimeId, target.RunTimeId);
        await UniTask.Yield();
    }
}
