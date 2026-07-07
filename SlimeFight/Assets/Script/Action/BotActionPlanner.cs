#nullable enable

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BotActionPlanner
{
    EnemyData enemyData = null!;
    CharacterManager characterManager = null!;
    MapManager mapManager = null!;
    int activeCharacterRunTimeId;
    IReadOnlyList<CharacterAction> availableActions = null!;
    int plannedActionCount;

    public void Initialize(
        EnemyData aEnemyData,
        CharacterManager aCharacterManager,
        MapManager aMapManager,
        int aActiveCharacterRunTimeId,
        IReadOnlyList<CharacterAction> aAvailableActions)
    {
        enemyData = aEnemyData;
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        activeCharacterRunTimeId = aActiveCharacterRunTimeId;
        availableActions = aAvailableActions;
        plannedActionCount = 0;
    }

    public async UniTask<CharacterAction?> GetNextAction()
    {
        await UniTask.Yield();

        if (plannedActionCount >= enemyData.BotPlannerData.MaxActionsPerTurn)
            return null;

        foreach (var action in availableActions)
        {
            if (TryPrepareAttack(action))
                return action;
        }

        foreach (var action in availableActions)
        {
            if (TryPrepareMove(action))
                return action;
        }

        return null;
    }

    bool TryPrepareAttack(CharacterAction action)
    {
        if (action.TargetStrategy is not EnemyInRangeStrategy enemyInRangeStrategy) return false;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return false;
        if (!characterManager.TryGetClosestValidAttackTarget(
                activeCharacterRunTimeId,
                enemyInRangeStrategy.Range,
                out var target))
            return false;

        action.Reset();
        action.SetSelectedTargets(new List<ActionTarget>
        {
            new(target.Position, target.RunTimeId)
        });
        plannedActionCount++;
        return true;
    }

    bool TryPrepareMove(CharacterAction action)
    {
        if (action.TargetStrategy is not MoveRangeStrategy moveRangeStrategy) return false;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return false;
        if (!characterManager.TryGetCharacter(activeCharacterRunTimeId, out var activeCharacter)) return false;
        if (!characterManager.TryGetClosestOpponent(activeCharacterRunTimeId, out var target)) return false;

        var direction = target.Position - activeCharacter.Position;
        direction.y = 0f;
        var distance = direction.magnitude;
        if (distance <= Mathf.Epsilon) return false;

        var moveDistance = Mathf.Min(moveRangeStrategy.Range, distance);
        var movePosition = activeCharacter.Position + direction.normalized * moveDistance;
        if (!mapManager.IsPositionOnMap(movePosition)) return false;

        action.Reset();
        action.SetSelectedTargets(new List<ActionTarget>
        {
            new(movePosition)
        });
        plannedActionCount++;
        return true;
    }
}
