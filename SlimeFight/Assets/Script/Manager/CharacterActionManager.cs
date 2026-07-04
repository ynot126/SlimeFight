#nullable enable

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterActionManager : MonoBehaviour
{
    [SerializeField] CharacterActionDisplay characterActionDisplayPrefab = null!;

    // Managers
    CharacterManager characterManager = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;

    // Turn State
    UniTaskCompletionSource? planningCompletionSource;
    bool isEndTurnRequested;
    bool isAwaitingActionConfirmation;

    int activeCharacterRunTimeId;
    GameView activeGameView = null!;

    // Action State
    readonly List<CharacterAction> availableActions = new();
    CharacterAction? selectedAction;

    public void Initialize(CharacterManager aCharacterManager, MapManager aMapManager, InputManager aInputManager)
    {
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        inputManager = aInputManager;
        var display = Instantiate(characterActionDisplayPrefab);
        display.SetVisible(false);
        characterManager.SetTargetSelectDisplay(display);
    }

    void OnDestroy()
    {
        inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;
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
        inputManager.OnMouseClick += HandleMouseClick;
        inputManager.OnMousePositionUpdate += HandleMousePositionUpdate;

        try
        {
            planningCompletionSource = new UniTaskCompletionSource();
            await planningCompletionSource.Task;
        }
        finally
        {
            isAwaitingActionConfirmation = false;
            inputManager.OnMouseClick -= HandleMouseClick;
            inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;
            HideActionRangeIndicator();
            selectedAction?.TargetStrategy.HideTargetDisplay();
            planningCompletionSource = null;
        }
    }

    void CompletePlanning() => planningCompletionSource?.TrySetResult();

    #endregion

    #region Action Selection

    void SelectAction(CharacterAction action)
    {
        if (!isAwaitingActionConfirmation) return;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return;

        action.Reset();
        selectedAction = action;
        UpdateActionButtonSelection();
        UpdateActionRangeIndicator();
        switch (selectedAction.TargetStrategy)
        {
            case MouseTargetSelectStrategy mouseStrategy:
                mouseStrategy.UpdateTargetDisplay(inputManager.CurrentMousePosition);
                break;
            case AutoTargetSelectStrategy:
                selectedAction.TargetStrategy.HideTargetDisplay();
                if (selectedAction.TryAutoSelectTarget())
                    CompletePlanning();
                break;
        }
    }

    async UniTask ExecuteSelectedAction()
    {
        if (selectedAction is not { HasSelectedTarget: true }) return;
        if (!characterManager.TrySpendMana(activeCharacterRunTimeId, selectedAction.ManaCost)) return;

        await selectedAction.ExecuteAsync();
        selectedAction.TargetStrategy.HideTargetDisplay();
        selectedAction = null;
        UpdateActionButtonSelection();
        UpdateActionRangeIndicator();
        UpdateManaDisplay(activeCharacterRunTimeId);
        UpdateActionButtonAffordability();
    }

    CharacterAction CreateAction(string actionId, int runTimeId)
        => new CharacterAction(ActionLibrary.GetAction(actionId), characterManager, mapManager, runTimeId);

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

    void HandleMousePositionUpdate(Vector3 mousePosition)
    {
        if (selectedAction?.TargetStrategy is not MouseTargetSelectStrategy mouseStrategy) return;
        mouseStrategy.UpdateTargetDisplay(mousePosition);
    }

    void HandleMouseClick(Vector3 mousePosition)
    {
        if (selectedAction == null) return;
        if (selectedAction.TargetStrategy is not MouseTargetSelectStrategy) return;
        if (!selectedAction.TrySelectTarget(mousePosition)) return;

        CompletePlanning();
    }

    void HandleEndTurnButtonPressed()
    {
        isEndTurnRequested = true;
        CompletePlanning();
    }

    #endregion
}
