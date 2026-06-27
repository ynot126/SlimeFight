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
    UniTaskCompletionSource characterActionTask = null!;
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
        characterActionTask = new UniTaskCompletionSource();
        
        // set the function
        gameView.SetShowCharacterActionOption(true);
        gameView.OnMoveButtonPressed += HandleGameViewMovementButtonPressed;
        inputManager.OnMousePositionUpdate += HandleMousePositionUpdate;
        characterManager.SetCharacterReadyAction(true,runTimeID);
        
        await characterActionTask.Task; 
        
        // clean up
        inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;
        gameView.OnMoveButtonPressed -= HandleGameViewMovementButtonPressed;
        characterManager.SetCharacterReadyAction(false,runTimeID);
        gameView.SetShowCharacterActionOption(false);
    }

    void HandleGameViewMovementButtonPressed()
    {
        
    }

    void HandleMousePositionUpdate(Vector2 mousePosition)
    {
        
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
