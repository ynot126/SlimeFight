#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameDrive : MonoBehaviour
{
    [Header("Managers")] 
    [SerializeField] SingletonSpawner singletonSpawnerPrefab = null!;
    [SerializeField] InputManager inputManagerPrefab = null!;
    [SerializeField] MapManager mapManagerPrefab = null!;
    [SerializeField] CharacterManager characterManagerPrefab = null!;
    
    [Header("Views")]
    [SerializeField] GameView gameViewPrefab = null!;
    
    // Managers
    InputManager inputManager = null!;
    MapManager mapManager = null!;
    CharacterManager characterManager = null!;
    
    // data
    GameData gameData = null!;
    
    //view
    GameView gameView = null!;
    
    // cts
    UniTaskCompletionSource characterSelectActionTask = null!;
    UniTaskCompletionSource characterActionTargetSelectionTask = null!;
    bool isEndTurnButtonClicked;
    
    // action data
    Vector2 actionTargetPosition = Vector2.zero;
    void Start()
    {
        Initialize();
        StartAsync().Forget();
    }

    void Initialize()
    {
        var singletonSpawner = Instantiate(singletonSpawnerPrefab);
        singletonSpawner.Initialize();
        
        inputManager = Instantiate(inputManagerPrefab);
        inputManager.Initialize(Camera.main!);

        gameData = DataManager.Instance.GetGameData();
        
        mapManager = Instantiate(mapManagerPrefab);
        mapManager.Initialize(gameData);
        
        characterManager = Instantiate(characterManagerPrefab);
        characterManager.Initialize(gameData , mapManager);
    }

    async UniTask StartAsync()
    {
        gameView = CreateGameView();
        ViewManager.Instance.PushView(gameView);
        
        await mapManager.CreateMap();
        await characterManager.SpawnCharacter();

        await GameLoop();
    }

    async UniTask GameLoop()
    {
        Debug.Log("Game Started");
        await UniTask.Yield();
        for (int x = 0; x < 10; x++)
        {
            await StartGameRound();
        }
    }

    async UniTask StartGameRound()
    {
        var orderList = characterManager.GetMovementOrder();
        foreach (var id in orderList)
        {
            await CharacterTurn(id);
        }
    }

    async UniTask CharacterTurn(int runTimeID)
    {
        
        // set the function
        gameView.SetShowCharacterActionOption(true);
        inputManager.OnMousePositionUpdate += HandleMousePositionUpdate;
        characterManager.SetCharacterReadyAction(true,runTimeID);

        isEndTurnButtonClicked = false;
        gameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;
        while (!isEndTurnButtonClicked)
        {
            
            characterSelectActionTask = new UniTaskCompletionSource();
            gameView.OnMoveButtonPressed += HandleGameViewMovementButtonPressed;
            await characterSelectActionTask.Task;
            inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;

            if (isEndTurnButtonClicked) break;
            
            characterActionTargetSelectionTask = new UniTaskCompletionSource();
            inputManager.OnMouseClick += HandleMouseOnClick;
            await characterActionTargetSelectionTask.Task;
            inputManager.OnMouseClick -= HandleMouseOnClick;

            if (isEndTurnButtonClicked) break;
            
            characterManager.CharacterMoveToPosition(runTimeID, actionTargetPosition);
        }
        gameView.OnEndTurnButtonPressed -= HandleEndTurnButtonPressed;
        
        // clean up
        gameView.OnMoveButtonPressed -= HandleGameViewMovementButtonPressed;
        characterManager.SetCharacterReadyAction(false,runTimeID);
        gameView.SetShowCharacterActionOption(false);
    }
    
    void HandleGameViewMovementButtonPressed()
    {
        characterSelectActionTask.TrySetResult();
    }

    void HandleMousePositionUpdate(Vector2 mousePosition)
    {
        
    }

    void HandleMouseOnClick(Vector2 mousePosition)
    {
        actionTargetPosition = mousePosition;
        characterActionTargetSelectionTask.TrySetResult();
    }

    void HandleEndTurnButtonPressed()
    {
        isEndTurnButtonClicked = true;
        characterSelectActionTask.TrySetResult();
        characterActionTargetSelectionTask.TrySetResult();
    }
    # region GameView
    GameView CreateGameView()
    {
        var view = Instantiate(gameViewPrefab);
        view.Initialize();
        return view;
    }
    #endregion
}
