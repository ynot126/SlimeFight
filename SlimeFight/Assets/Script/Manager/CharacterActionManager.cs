#nullable enable

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterActionManager : MonoBehaviour
{
    [SerializeField] CharacterActionDisplay characterActionDisplayPrefab = null!;

    CharacterActionDisplay targetSelectDisplay = null!;

    CharacterManager characterManager = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;

    bool isEndTurnRequested;
    bool isAwaitingActionConfirmation;

    int activeCharacterRunTimeId;
    GameView activeGameView = null!;

    readonly List<CharacterAction> availableActions = new();
    CharacterAction? selectedAction;

    CancellationTokenSource? actionConfirmationCts;
    UniTaskCompletionSource? actionSelectionSource;
    System.Action? cancelCurrentTargetSelection;

    public void Initialize(CharacterManager aCharacterManager, MapManager aMapManager, InputManager aInputManager)
    {
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        inputManager = aInputManager;
        targetSelectDisplay = Instantiate(characterActionDisplayPrefab);
        targetSelectDisplay.SetVisible(false);
    }

    void OnDestroy()
    {
        if (targetSelectDisplay != null)
            Destroy(targetSelectDisplay.gameObject);
    }

    #region Turn Lifecycle

    public async UniTask RunCharacterTurn(int runTimeId, GameView gameView)
    {
        BeginTurn(runTimeId, gameView);

        while (!isEndTurnRequested)
        {
            await WaitForActionConfirmation();
            if (isEndTurnRequested) break;

            await ExecuteSelectedAction();
        }

        EndTurn();
    }

    void BeginTurn(int runTimeId, GameView gameView)
    {
        activeCharacterRunTimeId = runTimeId;
        activeGameView = gameView;
        isEndTurnRequested = false;
        selectedAction = null;
        availableActions.Clear();

        characterManager.RefillMana(runTimeId);
        UpdateManaDisplay(runTimeId);

        foreach (var actionId in characterManager.GetCharacterActions(runTimeId))
            availableActions.Add(CreateAction(actionId, runTimeId));

        activeGameView.SpawnActionButtons(availableActions);
        activeGameView.SetShowCharacterActionOption(true);
        activeGameView.OnActionSelected += SelectAction;
        activeGameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;
        
        characterManager.SetCharacterReadyAction(true, runTimeId);
        UpdateActionButtonSelection();
        UpdateActionButtonAffordability();
    }

    void EndTurn()
    {
        activeGameView.OnActionSelected -= SelectAction;
        activeGameView.OnEndTurnButtonPressed -= HandleEndTurnButtonPressed;
        HideActionRangeIndicator();
        characterManager.SetCharacterReadyAction(false, activeCharacterRunTimeId);
        activeGameView.ClearActionButtons();
        activeGameView.ClearManaText();
        activeGameView.SetShowCharacterActionOption(false);
        availableActions.Clear();

        activeCharacterRunTimeId = 0;
        activeGameView = null!;
        selectedAction = null;
    }

    #endregion

    #region Planning Phase

    async UniTask WaitForActionConfirmation()
    {
        isAwaitingActionConfirmation = true;
        actionConfirmationCts = new CancellationTokenSource();

        try
        {
            while (!isEndTurnRequested)
            {
                await WaitUntilActionSelected();
                if (isEndTurnRequested) return;

                if (await TrySelectTargetsForSelectedAction(actionConfirmationCts.Token))
                    return;
            }
        }
        finally
        {
            isAwaitingActionConfirmation = false;
            actionSelectionSource?.TrySetCanceled();
            actionSelectionSource = null;
            cancelCurrentTargetSelection?.Invoke();
            cancelCurrentTargetSelection = null;
            actionConfirmationCts?.Cancel();
            actionConfirmationCts?.Dispose();
            actionConfirmationCts = null;
            HideActionRangeIndicator();
        }
    }

    async UniTask WaitUntilActionSelected()
    {
        while (selectedAction == null && !isEndTurnRequested)
        {
            actionSelectionSource = new UniTaskCompletionSource();
            await actionSelectionSource.Task;
            actionSelectionSource = null;
        }
    }

    async UniTask<bool> TrySelectTargetsForSelectedAction(CancellationToken confirmationToken)
    {
        var action = selectedAction;
        if (action == null) return false;

        if (action.TargetStrategy is AutoTargetSelectStrategy)
            throw new System.NotImplementedException("AutoTargetSelectStrategy is not implemented yet.");

        if (action.TargetStrategy is not MouseTargetSelectStrategy mouseStrategy)
            return true;

        var result = await WaitForMouseTargets(mouseStrategy, confirmationToken);

        if (isEndTurnRequested || selectedAction != action || result == null) return false;

        action.SetSelectedTargets(result);
        return true;
    }

    async UniTask<List<ActionTarget>?> WaitForMouseTargets(
        MouseTargetSelectStrategy mouseStrategy,
        CancellationToken confirmationToken)
    {
        using var targetSelectionCts = CancellationTokenSource.CreateLinkedTokenSource(confirmationToken);
        System.Action cancelTargetSelection = targetSelectionCts.Cancel;

        cancelCurrentTargetSelection = cancelTargetSelection;

        try
        {
            return await mouseStrategy.GetTarget(targetSelectionCts.Token);
        }
        finally
        {
            if (cancelCurrentTargetSelection == cancelTargetSelection)
                cancelCurrentTargetSelection = null;
        }
    }

    #endregion

    #region Action Selection

    void SelectAction(CharacterAction action)
    {
        if (!isAwaitingActionConfirmation) return;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return;

        var previousTargetSelection = cancelCurrentTargetSelection;
        selectedAction = null;
        previousTargetSelection?.Invoke();

        action.Reset();
        selectedAction = action;
        UpdateActionButtonSelection();
        UpdateActionRangeIndicator();
        actionSelectionSource?.TrySetResult();
    }

    async UniTask ExecuteSelectedAction()
    {
        if (selectedAction is not { HasSelectedTarget: true }) return;
        if (!characterManager.TrySpendMana(activeCharacterRunTimeId, selectedAction.ManaCost)) return;

        await selectedAction.ExecuteAsync();
        selectedAction.Reset();
        selectedAction = null;
        UpdateActionButtonSelection();
        UpdateActionRangeIndicator();
        UpdateManaDisplay(activeCharacterRunTimeId);
        UpdateActionButtonAffordability();
    }

    CharacterAction CreateAction(string actionId, int runTimeId)
        => new CharacterAction(
            ActionLibrary.GetAction(actionId),
            characterManager,
            mapManager,
            inputManager,
            runTimeId,
            targetSelectDisplay);

    #endregion

    #region UI

    void UpdateActionButtonSelection()
    {
        activeGameView.UpdateActionButtonSelection(selectedAction);
    }

    void UpdateManaDisplay(int runTimeId)
    {
        activeGameView.SetManaText(
            characterManager.GetCurrentMana(runTimeId),
            characterManager.GetMaxMana(runTimeId));
    }

    void UpdateActionButtonAffordability()
    {
        activeGameView.UpdateActionButtonAffordability(characterManager.GetCurrentMana(activeCharacterRunTimeId));
    }

    void UpdateActionRangeIndicator()
    {
        if (selectedAction == null)
        {
            HideActionRangeIndicator();
            return;
        }

        if (!characterManager.TryGetCharacter(activeCharacterRunTimeId, out var character))
        {
            HideActionRangeIndicator();
            return;
        }

        targetSelectDisplay.SetActionRangeIndicator(character.Position, selectedAction.TargetStrategy);
    }

    void HideActionRangeIndicator()
    {
        targetSelectDisplay.SetActionRangeIndicatorVisible(false);
    }

    void HandleEndTurnButtonPressed()
    {
        isEndTurnRequested = true;
        actionConfirmationCts?.Cancel();
        cancelCurrentTargetSelection?.Invoke();
        actionSelectionSource?.TrySetResult();
    }

    #endregion
}
