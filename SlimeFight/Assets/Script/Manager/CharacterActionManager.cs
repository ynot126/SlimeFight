#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum CharacterActionType
{
    Move,
    Attack,
}

public class CharacterActionManager : MonoBehaviour
{
    CharacterManager characterManager = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;

    UniTaskCompletionSource chooseActionTask = null!;
    UniTaskCompletionSource selectTargetTask = null!;
    bool isEndTurnRequested;

    CharacterActionType selectedAction;
    Vector2 moveTargetPosition;
    int attackTargetRunTimeId;

    int activeCharacterRunTimeId;
    GameView activeGameView= null!;

    public void Initialize(CharacterManager aCharacterManager, MapManager aMapManager, InputManager aInputManager)
    {
        characterManager = aCharacterManager;
        mapManager = aMapManager;
        inputManager = aInputManager;
    }

    public async UniTask RunCharacterTurn(int runTimeId, GameView gameView)
    {
        activeCharacterRunTimeId = runTimeId;
        activeGameView = gameView;

        gameView.SetShowCharacterActionOption(true);
        characterManager.SetCharacterReadyAction(true, runTimeId);
        isEndTurnRequested = false;

        gameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;

        while (!isEndTurnRequested)
        {
            await ChooseActionPhase();
            if (isEndTurnRequested) break;

            await SelectTargetPhase();
            if (isEndTurnRequested) break;

            await ExecuteActionPhase();
        }

        gameView.OnEndTurnButtonPressed -= HandleEndTurnButtonPressed;
        activeGameView.OnMoveButtonPressed -= HandleMoveButtonPressed;
        activeGameView.OnAttackButtonPressed -= HandleAttackButtonPressed;
        characterManager.SetCharacterReadyAction(false, runTimeId);
        gameView.SetShowCharacterActionOption(false);

        activeGameView = null;
    }

    async UniTask ChooseActionPhase()
    {
        chooseActionTask = new UniTaskCompletionSource();
        activeGameView.OnMoveButtonPressed += HandleMoveButtonPressed;
        activeGameView.OnAttackButtonPressed += HandleAttackButtonPressed;
        await chooseActionTask.Task;
        activeGameView.OnMoveButtonPressed -= HandleMoveButtonPressed;
        activeGameView.OnAttackButtonPressed -= HandleAttackButtonPressed;
    }

    async UniTask SelectTargetPhase()
    {
        selectTargetTask = new UniTaskCompletionSource();
        inputManager.OnMouseClick += HandleMouseClick;
        await selectTargetTask.Task;
        inputManager.OnMouseClick -= HandleMouseClick;
    }

    async UniTask ExecuteActionPhase()
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

    void HandleMoveButtonPressed()
    {
        selectedAction = CharacterActionType.Move;
        chooseActionTask.TrySetResult();
    }

    void HandleAttackButtonPressed()
    {
        selectedAction = CharacterActionType.Attack;
        chooseActionTask.TrySetResult();
    }

    void HandleMouseClick(Vector2 mousePosition)
    {
        switch (selectedAction)
        {
            case CharacterActionType.Move:
                if (!mapManager.IsPositionOnMap(mousePosition)) return;
                moveTargetPosition = mousePosition;
                selectTargetTask.TrySetResult();
                break;
            case CharacterActionType.Attack:
                if (!characterManager.TryGetCharacterAtPosition(mousePosition, activeCharacterRunTimeId, out var target)) return;
                if (!characterManager.IsValidAttackTarget(activeCharacterRunTimeId, target.RunTimeId)) return;
                attackTargetRunTimeId = target.RunTimeId;
                selectTargetTask.TrySetResult();
                break;
        }
    }

    void HandleEndTurnButtonPressed()
    {
        isEndTurnRequested = true;
        chooseActionTask.TrySetResult();
        selectTargetTask.TrySetResult();
    }
}
