#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterAction
{
    readonly ActionData data;
    readonly ActionContext context;

    public string ActionName => data.Id;
    public int ManaCost => data.ManaCost;
    public float ActionRange => data.BaseTargetStrategy.Range;
    public BaseTargetSelectStrategy TargetStrategy => data.BaseTargetStrategy;
    public bool HasSelectedTarget => hasSelectedTarget;
    public Vector3 TargetPosition => selectedTarget.Position;

    bool hasSelectedTarget;

    ActionTarget selectedTarget;

    public CharacterAction(
        ActionData actionData,
        CharacterManager characterManager,
        MapManager mapManager,
        int activeCharacterRunTimeId,
        CharacterActionDisplay? targetSelectDisplay = null)
    {
        data = actionData;
        context = new ActionContext(characterManager, mapManager, activeCharacterRunTimeId);
        if (data.BaseTargetStrategy is MouseTargetSelectStrategy mouseStrategy && targetSelectDisplay != null)
            mouseStrategy.Initialize(context, targetSelectDisplay);
        else
            data.BaseTargetStrategy.Initialize(context);
    }

    public void Reset()
    {
        hasSelectedTarget = false;
        selectedTarget = default;
    }

    public bool IsValidTargetAt(Vector3 mousePosition)
    {
        if (data.BaseTargetStrategy is not MouseTargetSelectStrategy mouseStrategy) return false;
        return mouseStrategy.TryGetTarget(mousePosition, out _);
    }

    public bool TrySelectTarget(Vector3 mousePosition)
    {
        if (data.BaseTargetStrategy is not MouseTargetSelectStrategy mouseStrategy) return false;
        if (!mouseStrategy.TryGetTarget(mousePosition, out var target)) return false;

        selectedTarget = target;
        hasSelectedTarget = true;
        return true;
    }

    public bool TryAutoSelectTarget()
    {
        if (data.BaseTargetStrategy is not AutoTargetSelectStrategy) return false;
        if (!data.BaseTargetStrategy.TryGetTarget(out var target)) return false;

        selectedTarget = target;
        hasSelectedTarget = true;
        return true;
    }

    public UniTask ExecuteAsync() => data.Execution.ExecuteAsync(context, selectedTarget);
}
