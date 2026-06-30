#nullable enable
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

    MoveAction moveAction = null!;
    AttackAction attackAction = null!;
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

        moveAction = new MoveAction(characterManager, mapManager, runTimeId);
        attackAction = new AttackAction(characterManager, mapManager, runTimeId);

        gameView.SetShowCharacterActionOption(true);
        characterManager.SetCharacterReadyAction(true, runTimeId);
        activeGameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;
    }

    void EndTurn(int runTimeId, GameView gameView)
    {
        activeGameView.OnEndTurnButtonPressed -= HandleEndTurnButtonPressed;
        characterManager.SetCharacterReadyAction(false, runTimeId);
        gameView.SetShowCharacterActionOption(false);

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
                activeGameView.OnMoveButtonPressed += HandleMoveButtonPressed;
                activeGameView.OnAttackButtonPressed += HandleAttackButtonPressed;
                inputManager.OnMouseClick += HandleMouseClick;
                break;
        }
    }

    void OnStateExit(CharacterTurnState state)
    {
        switch (state)
        {
            case CharacterTurnState.PlanningAction:
                activeGameView.OnMoveButtonPressed -= HandleMoveButtonPressed;
                activeGameView.OnAttackButtonPressed -= HandleAttackButtonPressed;
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
        await selectedAction.ExecuteAsync();
        selectedAction = null;
    }

    void SelectAction(CharacterAction action)
    {
        if (currentState != CharacterTurnState.PlanningAction) return;

        action.Reset();
        selectedAction = action;
    }

    void HandleMoveButtonPressed()
    {
        SelectAction(moveAction);
    }

    void HandleAttackButtonPressed()
    {
        SelectAction(attackAction);
    }

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
