#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameDrive : MonoBehaviour
{
    [Header("Managers")] 
    [SerializeField] SingletonSpawner singletonSpawnerPrefab = null!;
    [SerializeField] InputManager inputManagerPrefab = null!;
    [SerializeField] MapManager mapManagerPrefab = null!;
    [SerializeField] CharacterManager characterManagerPrefab = null!;
    [SerializeField] CharacterActionManager characterActionManagerPrefab = null!;
    [SerializeField] BotActionManager botActionManagerPrefab = null!;

    [Header("Camera")]
    [SerializeField] GameCameraController gameCameraController = null!;

    [Header("Views")] 
    [SerializeField] GameView gameViewPrefab = null!;


    // Managers
    InputManager inputManager = null!;
    MapManager mapManager = null!;
    CharacterManager characterManager = null!;
    CharacterActionManager characterActionManager = null!;
    BotActionManager botActionManager = null!;

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
        gameCameraController.Initialize(inputManager, new Bounds(Vector3.zero, new Vector3(10f, 1f, 10f)));
        
        gameData = DataManager.Instance.GetGameData();

        mapManager = Instantiate(mapManagerPrefab);
        mapManager.Initialize(gameData);
        
        characterManager = Instantiate(characterManagerPrefab);
        characterManager.Initialize(gameData, mapManager, inputManager, Camera.main!);
        
        characterActionManager = Instantiate(characterActionManagerPrefab);
        characterActionManager.Initialize(characterManager, mapManager, inputManager);

        botActionManager = botActionManagerPrefab != null
            ? Instantiate(botActionManagerPrefab)
            : new GameObject(nameof(BotActionManager)).AddComponent<BotActionManager>();
        botActionManager.Initialize(characterManager, mapManager, inputManager);
    }


    async UniTask StartAsync()
    {
        gameView = CreateGameView();

        ViewManager.Instance.PushView(gameView);
        
        await mapManager.CreateMap();
        await characterManager.SpawnCharacter();
        await characterManager.SpawnEnemy();
        await GameLoop();
    }


    async UniTask GameLoop()
    {
        Debug.Log("Game Started");
        gameCameraController.SetDraggingEnabled(true);
        await UniTask.Yield();
        for (int round = 1; round <= 10; round++)
        {
            await StartGameRound(round);
        }
    }

    async UniTask StartGameRound(int round)
    {
        gameView.SetRoundText(round);
        var orderList = characterManager.GetMovementOrder();
        foreach (var id in orderList)
        {
            if (characterManager.GetCharacterType(id) == CharacterType.Player)
                await characterActionManager.RunCharacterTurn(id, gameView);
            else
                await botActionManager.RunCharacterTurn(id, gameView);
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