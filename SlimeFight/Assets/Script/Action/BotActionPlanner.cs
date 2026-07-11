#nullable enable

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BotActionPlanner
{
    BotData botData = null!;
    CharacterManager characterManager = null!;
    MapManager mapManager = null!;
    int activeCharacterRunTimeId;
    IReadOnlyList<CharacterAction> availableActions = null!;
    int plannedActionCount;

    public void Initialize(
        BotData aBotData,
        CharacterManager aCharacterManager,
        MapManager aMapManager,
        int aActiveCharacterRunTimeId,
        IReadOnlyList<CharacterAction> aAvailableActions)
    {
        botData = aBotData;
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        activeCharacterRunTimeId = aActiveCharacterRunTimeId;
        availableActions = aAvailableActions;
        plannedActionCount = 0;
    }

    public async UniTask<CharacterAction?> GetNextAction()
    {
        await UniTask.Yield();

        if (plannedActionCount >= botData.BotPlannerData.MaxActionsPerTurn)
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

        if (!characterManager.TryGetCharacterHex(target.RunTimeId, out var targetHex)) return false;

        action.Reset();
        action.SetSelectedTargets(new List<ActionTarget>
        {
            new(targetHex, target.Position, target.RunTimeId)
        });
        plannedActionCount++;
        return true;
    }

    bool TryPrepareMove(CharacterAction action)
    {
        if (action.TargetStrategy is not MoveRangeStrategy moveRangeStrategy) return false;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return false;
        if (!characterManager.TryGetCharacterHex(activeCharacterRunTimeId, out var activeHex)) return false;
        if (!characterManager.TryGetClosestOpponent(activeCharacterRunTimeId, out var target)) return false;
        if (!characterManager.TryGetCharacterHex(target.RunTimeId, out var targetHex)) return false;

        var reachableHexes = characterManager.GetReachableHexes(activeCharacterRunTimeId, Mathf.RoundToInt(moveRangeStrategy.Range));
        var currentDistance = mapManager.GetHexDistance(activeHex, targetHex);
        var bestDistance = currentDistance;
        var bestHex = activeHex;
        foreach (var pair in reachableHexes)
        {
            var candidateHex = pair.Key;
            if (candidateHex == activeHex) continue;

            var distance = mapManager.GetHexDistance(candidateHex, targetHex);
            if (distance >= bestDistance) continue;

            bestDistance = distance;
            bestHex = candidateHex;
        }

        if (bestHex == activeHex) return false;

        action.Reset();
        action.SetSelectedTargets(new List<ActionTarget>
        {
            new(bestHex, mapManager.HexToWorld(bestHex))
        });
        plannedActionCount++;
        return true;
    }
}
