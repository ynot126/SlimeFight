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

    CancellationTokenSource? turnCts;
    CancellationTokenSource? targetSelectionCts;
    UniTaskCompletionSource? actionSelectionSource;

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
        UpdateActionButtonSelection();
        UpdateActionButtonAffordability();
        characterManager.SetCharacterReadyAction(true, runTimeId);
        activeGameView.OnActionSelected += SelectAction;
        activeGameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;
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
        turnCts = new CancellationTokenSource();

        try
        {
            while (!isEndTurnRequested)
            {
                if (selectedAction == null)
                {
                    actionSelectionSource = new UniTaskCompletionSource();
                    await actionSelectionSource.Task;
                    actionSelectionSource = null;
                    if (isEndTurnRequested) return;
                }

                if (selectedAction!.TargetStrategy is AutoTargetSelectStrategy)
                    throw new NotImplementedException("AutoTargetSelectStrategy is not implemented yet.");

                if (selectedAction.TargetStrategy is not MouseTargetSelectStrategy mouseStrategy)
                    return;

                targetSelectionCts?.Cancel();
                targetSelectionCts?.Dispose();
                targetSelectionCts = CancellationTokenSource.CreateLinkedTokenSource(turnCts.Token);

                var result = await mouseStrategy.GetTarget(targetSelectionCts.Token);

                if (isEndTurnRequested) return;

                if (result != null)
                {
                    selectedAction.SetSelectedTargets(result);
                    return;
                }
            }
        }
        finally
        {
            isAwaitingActionConfirmation = false;
            actionSelectionSource?.TrySetCanceled();
            actionSelectionSource = null;
            targetSelectionCts?.Cancel();
            targetSelectionCts?.Dispose();
            targetSelectionCts = null;
            turnCts?.Cancel();
            turnCts?.Dispose();
            turnCts = null;
            HideActionRangeIndicator();
        }
    }

    #endregion

    #region Action Selection

    void SelectAction(CharacterAction action)
    {
        if (!isAwaitingActionConfirmation) return;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return;

        targetSelectionCts?.Cancel();

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
        if (selectedAction != null)
            characterManager.SetActionRangeIndicator(activeCharacterRunTimeId, selectedAction.ActionRange, true);
        else
            HideActionRangeIndicator();
    }

    void HideActionRangeIndicator()
    {
        characterManager.SetActionRangeIndicator(activeCharacterRunTimeId, 0f, false);
    }

    void HandleEndTurnButtonPressed()
    {
        isEndTurnRequested = true;
        turnCts?.Cancel();
        targetSelectionCts?.Cancel();
        actionSelectionSource?.TrySetResult();
    }

    #endregion
}
