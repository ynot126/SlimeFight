#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterAction
{
    readonly ActionData data;
    readonly ActionContext context;

    public string ActionName => data.Id;
    public int ManaCost => data.ManaCost;
    public float ActionRange => data.TargetStrategy.Range;
    public bool HasSelectedTarget => hasSelectedTarget;
    public Vector2 TargetPosition => selectedTarget.Position;

    bool hasSelectedTarget;

    ActionTarget selectedTarget;

    public CharacterAction(ActionData actionData, CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
    {
        data = actionData;
        context = new ActionContext(characterManager, mapManager, activeCharacterRunTimeId);
    }

    public void Reset()
    {
        hasSelectedTarget = false;
        selectedTarget = default;
    }

    public bool TrySelectTarget(Vector2 mousePosition)
    {
        if (!data.TargetStrategy.TrySelectTarget(context, mousePosition, out var target))
            return false;

        selectedTarget = target;
        hasSelectedTarget = true;
        return true;
    }

    public UniTask ExecuteAsync() => data.Execution.ExecuteAsync(context, selectedTarget);
}
