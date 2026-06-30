#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum CharacterActionType
{
    Move,
    Attack,
}

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

    CharacterActionType selectedAction;
    bool hasSelectedAction;
    Vector2 moveTargetPosition;
    int attackTargetRunTimeId;

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
        hasSelectedAction = false;

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
        hasSelectedAction = false;
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
        switch (selectedAction)
        {
            case CharacterActionType.Move:
                await characterManager.CharacterMoveToPosition(activeCharacterRunTimeId, moveTargetPosition);
                break;
            case CharacterActionType.Attack:
                characterManager.CharacterAttack(activeCharacterRunTimeId, attackTargetRunTimeId);
                await UniTask.Yield();
                break;
        }
    }

    void SelectAction(CharacterActionType action)
    {
        if (currentState != CharacterTurnState.PlanningAction) return;

        selectedAction = action;
        hasSelectedAction = true;
    }

    void HandleMoveButtonPressed()
    {
        SelectAction(CharacterActionType.Move);
    }

    void HandleAttackButtonPressed()
    {
        SelectAction(CharacterActionType.Attack);
    }

    void HandleMouseClick(Vector2 mousePosition)
    {
        if (currentState != CharacterTurnState.PlanningAction || !hasSelectedAction) return;

        switch (selectedAction)
        {
            case CharacterActionType.Move:
                if (!mapManager.IsPositionOnMap(mousePosition)) return;
                moveTargetPosition = mousePosition;
                CompleteCurrentState();
                break;
            case CharacterActionType.Attack:
                if (!characterManager.TryGetCharacterAtPosition(mousePosition, activeCharacterRunTimeId, out var target)) return;
                if (!characterManager.IsValidAttackTarget(activeCharacterRunTimeId, target.RunTimeId)) return;
                attackTargetRunTimeId = target.RunTimeId;
                CompleteCurrentState();
                break;
        }
    }

    void HandleEndTurnButtonPressed()
    {
        isEndTurnRequested = true;
        CompleteCurrentState();
    }
}
