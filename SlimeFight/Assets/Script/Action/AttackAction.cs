#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttackAction : CharacterAction
{
    int targetRunTimeId;

    public AttackAction(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
        : base(characterManager, mapManager, activeCharacterRunTimeId) { }

    public override CharacterActionType ActionType => CharacterActionType.Attack;

    protected override void OnReset() => targetRunTimeId = 0;

    public override bool TrySelectTarget(Vector2 mousePosition)
    {
        if (!CharacterManager.TryGetCharacterAtPosition(mousePosition, ActiveCharacterRunTimeId, out var target))
            return false;
        if (!CharacterManager.IsValidAttackTarget(ActiveCharacterRunTimeId, target.RunTimeId))
            return false;

        targetRunTimeId = target.RunTimeId;
        HasSelectedTarget = true;
        return true;
    }

    public override async UniTask ExecuteAsync()
    {
        CharacterManager.CharacterAttack(ActiveCharacterRunTimeId, targetRunTimeId);
        await UniTask.Yield();
    }
}
