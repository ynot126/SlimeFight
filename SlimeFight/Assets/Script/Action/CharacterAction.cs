#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterAction
{
    readonly ActionData data;
    readonly ActionContext context;

    public string ActionName => data.Id;
    public int ManaCost => data.ManaCost;
    public bool HasSelectedTarget { get; private set; }
    public Vector2 TargetPosition => selectedTarget.Position;

    ActionTarget selectedTarget;

    public CharacterAction(ActionData actionData, CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
    {
        data = actionData;
        context = new ActionContext(characterManager, mapManager, activeCharacterRunTimeId);
    }

    public void Reset()
    {
        HasSelectedTarget = false;
        selectedTarget = default;
    }

    public bool TrySelectTarget(Vector2 mousePosition)
    {
        if (!data.TargetStrategy.TrySelectTarget(context, mousePosition, out var target))
            return false;

        selectedTarget = target;
        HasSelectedTarget = true;
        return true;
    }

    public UniTask ExecuteAsync() => data.Execution.ExecuteAsync(context, selectedTarget);
}
