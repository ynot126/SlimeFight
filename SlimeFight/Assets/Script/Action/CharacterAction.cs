#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum CharacterActionType
{
    Move,
    Attack,
}

public abstract class CharacterAction
{
    protected CharacterManager CharacterManager { get; }
    protected MapManager MapManager { get; }
    protected int ActiveCharacterRunTimeId { get; }

    protected CharacterAction(CharacterManager characterManager, MapManager mapManager, int activeCharacterRunTimeId)
    {
        CharacterManager = characterManager;
        MapManager = mapManager;
        ActiveCharacterRunTimeId = activeCharacterRunTimeId;
    }

    public abstract CharacterActionType ActionType { get; }
    public bool HasSelectedTarget { get; protected set; }

    public void Reset()
    {
        HasSelectedTarget = false;
        OnReset();
    }

    protected virtual void OnReset() { }

    public abstract bool TrySelectTarget(Vector2 mousePosition);
    public abstract UniTask ExecuteAsync();
}
