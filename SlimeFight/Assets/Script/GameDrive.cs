#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameDrive : MonoBehaviour
{
    [Header("Managers")] [SerializeField] SingletonSpawner singletonSpawnerPrefab = null!;
    [SerializeField] InputManager inputManagerPrefab = null!;
    [SerializeField] MapManager mapManagerPrefab = null!;
    [SerializeField] CharacterManager characterManagerPrefab = null!;
    [SerializeField] CharacterActionManager characterActionManagerPrefab = null!;

    [Header("Views")] [SerializeField] GameView gameViewPrefab = null!;


    // Managers
    InputManager inputManager = null!;
    MapManager mapManager = null!;
    CharacterManager characterManager = null!;
    CharacterActionManager characterActionManager = null!;

    // data
    GameData gameData = null!;

    //view
    GameView gameView = null!;

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
        characterManager.Initialize(gameData, mapManager, inputManager);
        
        characterActionManager = Instantiate(characterActionManagerPrefab);
        characterActionManager.Initialize(characterManager, mapManager, inputManager);
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
            await characterActionManager.RunCharacterTurn(id, gameView);
        }
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