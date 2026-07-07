#nullable enable

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class BaseCharacterActionManager : MonoBehaviour
{
    [SerializeField] CharacterActionDisplay characterActionDisplayPrefab = null!;

    protected CharacterActionDisplay TargetSelectDisplay { get; private set; } = null!;
    protected CharacterManager CharacterManager { get; private set; } = null!;
    protected MapManager MapManager { get; private set; } = null!;
    protected InputManager InputManager { get; private set; } = null!;
    protected int ActiveCharacterRunTimeId { get; private set; }
    protected GameView ActiveGameView { get; private set; } = null!;
    protected IReadOnlyList<CharacterAction> AvailableActions => availableActions;
    protected bool IsEndTurnRequested { get; private set; }

    readonly List<CharacterAction> availableActions = new();

    public virtual void Initialize(CharacterManager aCharacterManager, MapManager aMapManager, InputManager aInputManager)
    {
        CharacterManager = aCharacterManager;
        MapManager = aMapManager;
        InputManager = aInputManager;
        if (characterActionDisplayPrefab == null) return;

        TargetSelectDisplay = Instantiate(characterActionDisplayPrefab);
        TargetSelectDisplay.SetVisible(false);
    }

    protected virtual void OnDestroy()
    {
        if (TargetSelectDisplay != null)
            Destroy(TargetSelectDisplay.gameObject);
    }

    public async UniTask RunCharacterTurn(int runTimeId, GameView gameView)
    {
        BeginTurn(runTimeId, gameView);

        try
        {
            while (!IsEndTurnRequested)
            {
                var action = await GetNextAction();
                if (IsEndTurnRequested || action == null) break;

                await ExecuteAction(action);
            }
        }
        finally
        {
            EndTurn();
        }
    }

    protected virtual void BeginTurn(int runTimeId, GameView gameView)
    {
        ActiveCharacterRunTimeId = runTimeId;
        ActiveGameView = gameView;
        IsEndTurnRequested = false;
        availableActions.Clear();

        CharacterManager.RefillMana(runTimeId);

        foreach (var actionId in CharacterManager.GetCharacterActions(runTimeId))
            availableActions.Add(CreateAction(actionId, runTimeId));

        CharacterManager.SetCharacterReadyAction(true, runTimeId);
    }

    protected virtual void EndTurn()
    {
        HideActionRangeIndicator();
        CharacterManager.SetCharacterReadyAction(false, ActiveCharacterRunTimeId);
        availableActions.Clear();

        ActiveCharacterRunTimeId = 0;
        ActiveGameView = null!;
        IsEndTurnRequested = false;
    }

    protected abstract UniTask<CharacterAction?> GetNextAction();

    protected virtual async UniTask ExecuteAction(CharacterAction action)
    {
        if (!action.HasSelectedTarget) return;
        if (!CharacterManager.TrySpendMana(ActiveCharacterRunTimeId, action.ManaCost)) return;

        await action.ExecuteAsync();
        action.Reset();
        OnActionExecuted();
    }

    protected virtual void OnActionExecuted()
    {
    }

    protected CharacterAction CreateAction(string actionId, int runTimeId)
        => new CharacterAction(
            ActionLibrary.GetAction(actionId),
            CharacterManager,
            MapManager,
            InputManager,
            runTimeId,
            TargetSelectDisplay);

    protected void RequestEndTurn()
    {
        IsEndTurnRequested = true;
    }

    protected virtual void HideActionRangeIndicator()
    {
        if (TargetSelectDisplay == null) return;

        TargetSelectDisplay.SetActionRangeIndicatorVisible(false);
    }
}
