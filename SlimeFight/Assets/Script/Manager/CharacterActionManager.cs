#nullable enable

using System;
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
    CharacterManager characterManager = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;

    CharacterTurnState currentState = CharacterTurnState.Inactive;
    UniTaskCompletionSource? stateCompletionSource;
    bool isEndTurnRequested;

    readonly List<CharacterAction> availableActions = new();
    CharacterAction? selectedAction;

    int activeCharacterRunTimeId;
    GameView activeGameView = null!;

    public void Initialize(CharacterManager aCharacterManager, MapManager aMapManager, InputManager aInputManager)
    {
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        inputManager = aInputManager;
    }

    public async UniTask RunCharacterTurn(int runTimeId, GameView gameView)
    {
        BeginTurn(runTimeId, gameView);

        while (!isEndTurnRequested)
        {
            await RunState(CharacterTurnState.PlanningAction);
            if (isEndTurnRequested) break;

            await RunState(CharacterTurnState.ExecutingAction);
        }

        EndTurn(runTimeId, gameView);
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

        foreach (var actionType in characterManager.GetCharacterActions(runTimeId))
            availableActions.Add(CreateAction(actionType, runTimeId));

        gameView.SpawnActionButtons(availableActions, SelectAction);
        gameView.SetShowCharacterActionOption(true);
        UpdateActionButtonSelection();
        UpdateActionButtonAffordability();
        characterManager.SetCharacterReadyAction(true, runTimeId);
        activeGameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;
    }

    void EndTurn(int runTimeId, GameView gameView)
    {
        activeGameView.OnEndTurnButtonPressed -= HandleEndTurnButtonPressed;
        characterManager.SetCharacterReadyAction(false, runTimeId);
        gameView.ClearActionButtons();
        gameView.ClearManaText();
        gameView.SetShowCharacterActionOption(false);
        availableActions.Clear();

        activeCharacterRunTimeId = 0;
        activeGameView = null!;
        currentState = CharacterTurnState.Inactive;
        selectedAction = null;
    }

    async UniTask RunState(CharacterTurnState state)
    {
        currentState = state;
        OnStateEnter(state);

        if (state == CharacterTurnState.ExecutingAction)
        {
            await ExecuteSelectedAction();
        }
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
                break;
        }
    }

    void CompleteCurrentState()
    {
        stateCompletionSource?.TrySetResult();
    }

    async UniTask ExecuteSelectedAction()
    {
        if (selectedAction is not { HasSelectedTarget: true }) return;
        if (!characterManager.TrySpendMana(activeCharacterRunTimeId, selectedAction.ManaCost)) return;

        await selectedAction.ExecuteAsync();
        selectedAction = null;
        UpdateActionButtonSelection();
        UpdateManaDisplay(activeCharacterRunTimeId);
        UpdateActionButtonAffordability();
    }

    void SelectAction(CharacterAction action)
    {
        if (currentState != CharacterTurnState.PlanningAction) return;
        if (!characterManager.CanAffordMana(activeCharacterRunTimeId, action.ManaCost)) return;

        action.Reset();
        selectedAction = action;
        UpdateActionButtonSelection();
    }

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

    CharacterAction CreateAction(CharacterActionType type, int runTimeId) => type switch
    {
        CharacterActionType.Move => new MoveAction(characterManager, mapManager, runTimeId),
        CharacterActionType.Attack => new AttackAction(characterManager, mapManager, runTimeId),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    void HandleMouseClick(Vector2 mousePosition)
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
}
