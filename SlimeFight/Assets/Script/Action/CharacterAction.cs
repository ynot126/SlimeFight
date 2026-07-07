#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterAction
{
    readonly ActionData data;
    readonly CharacterManager characterManager;
    readonly MapManager mapManager;
    readonly int activeCharacterRunTimeId;

    readonly List<ActionTarget> selectedTargets = new();

    public string ActionName => data.Id;
    public int ManaCost => data.ManaCost;
    public float ActionRange => data.BaseTargetStrategy is MouseTargetSelectStrategy mouseStrategy
        ? mouseStrategy.Range
        : 0f;
    public BaseTargetSelectStrategy TargetStrategy => data.BaseTargetStrategy;
    public bool HasSelectedTarget => selectedTargets.Count > 0;
    public Vector3 TargetPosition => selectedTargets[0].Position;

    public CharacterAction(
        ActionData actionData,
        CharacterManager aCharacterManager,
        MapManager aMapManager,
        InputManager inputManager,
        int aActiveCharacterRunTimeId,
        CharacterActionDisplay targetSelectDisplay)
    {
        data = actionData;
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        activeCharacterRunTimeId = aActiveCharacterRunTimeId;
        data.BaseTargetStrategy.Initialize(
            aCharacterManager, aMapManager, inputManager, aActiveCharacterRunTimeId, targetSelectDisplay);
    }

    public void Reset()
    {
        selectedTargets.Clear();
    }

    public void SetSelectedTargets(List<ActionTarget> targets)
    {
        selectedTargets.Clear();
        selectedTargets.AddRange(targets);
    }

    public UniTask ExecuteAsync()
        => data.Execution.ExecuteAsync(characterManager, mapManager, activeCharacterRunTimeId, selectedTargets[0]);
}
