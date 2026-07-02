#nullable enable

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum CharacterTurnState
{
    Inactive,
    PlanningAction,
    ExecutingAction,
}

public class CharacterActionManager : MonoBehaviour
{
    // Managers
    CharacterManager characterManager = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;
    
    //Turn State
    CharacterTurnState currentState = CharacterTurnState.Inactive;
    UniTaskCompletionSource? stateCompletionSource;
    bool isEndTurnRequested;

    int activeCharacterRunTimeId;
    GameView activeGameView = null!;
    
    //Action State
    readonly List<CharacterAction> availableActions = new();
    CharacterAction? selectedAction;

    public void Initialize(CharacterManager aCharacterManager, MapManager aMapManager, InputManager aInputManager)
    {
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        inputManager = aInputManager;
    }
    
    #region Turn Lifecycle

    public async UniTask RunCharacterTurn(int runTimeId, GameView gameView)
    {
        BeginTurn(runTimeId, gameView);

        while (!isEndTurnRequested)
        {
            await RunState(CharacterTurnState.PlanningAction);
            if (isEndTurnRequested) break;

            await RunState(CharacterTurnState.ExecutingAction);
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

        activeGameView.SpawnActionButtons(availableActions, SelectAction);
        activeGameView.SetShowCharacterActionOption(true);
        UpdateActionButtonSelection();
        UpdateActionButtonAffordability();
        characterManager.SetCharacterReadyAction(true, runTimeId);
        activeGameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;
    }

    void EndTurn()
    {
        activeGameView.OnEndTurnButtonPressed -= HandleEndTurnButtonPressed;
        HideActionRangeIndicator();
        characterManager.SetCharacterReadyAction(false, activeCharacterRunTimeId);
        activeGameView.ClearActionButtons();
        activeGameView.ClearManaText();
        activeGameView.SetShowCharacterActionOption(false);
        availableActions.Clear();

        activeCharacterRunTimeId = 0;
        activeGameView = null!;
        currentState = CharacterTurnState.Inactive;
        selectedAction = null;
    }

    #endregion

    #region State Machine

    async UniTask RunState(CharacterTurnState state)
    {
        currentState = state;
        OnStateEnter(state);

        if (state == CharacterTurnState.ExecutingAction)
            await ExecuteSelectedAction();
        else
        {
            stateCompletionSource = new UniTaskCompletionSource();
            await stateCompletionSource.Task;
            stateCompletionSource = null;
        }

        OnStateExit(state);
        currentState = CharacterTurnState.Inactive;
    }

    void OnStateEnter(CharacterTurnState state)
    {
        switch (state)
        {
            case CharacterTurnState.PlanningAction:
                inputManager.OnMouseClick += HandleMouseClick;
                break;
        }
    }

    void OnStateExit(CharacterTurnState state)
    {
        switch (state)
        {
            case CharacterTurnState.PlanningAction:
                inputManager.OnMouseClick -= HandleMouseClick;
                HideActionRangeIndicator();
                break;
        }
    }

    void CompleteCurrentState() => stateCompletionSource?.TrySetResult();

    #endregion

    #region Action Selection & Execution

    void SelectAction(CharacterAction action)
    {
        if (currentState != CharacterTurnState.PlanningAction) return;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return;

        action.Reset();
        selectedAction = action;
        UpdateActionButtonSelection();
        UpdateActionRangeIndicator();
    }

    async UniTask ExecuteSelectedAction()
    {
        if (selectedAction is not { HasSelectedTarget: true }) return;
        if (!characterManager.TrySpendMana(activeCharacterRunTimeId, selectedAction.ManaCost)) return;

        await selectedAction.ExecuteAsync();
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

    #endregion

    #region Event Handlers

    void HandleMouseClick(Vector3 mousePosition)
    {
        if (currentState != CharacterTurnState.PlanningAction || selectedAction == null) return;
        if (!selectedAction.TrySelectTarget(mousePosition)) return;

        CompleteCurrentState();
    }

    void HandleEndTurnButtonPressed()
    {
        isEndTurnRequested = true;
        CompleteCurrentState();
    }

    #endregion
}
