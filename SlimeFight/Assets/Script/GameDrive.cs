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

    // Managers
    InputManager inputManager = null!;
    MapManager mapManager = null!;
    CharacterManager characterManager = null!;
    
    // data
    GameData gameData = null!;
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
        inputManager.Initialize();

        gameData = DataManager.Instance.GetGameData();
        
        mapManager = Instantiate(mapManagerPrefab);
        mapManager.Initialize(gameData);
        
        characterManager = Instantiate(characterManagerPrefab);
        characterManager.Initialize(gameData , mapManager);
    }

    async UniTask StartAsync()
    {
        await mapManager.CreateMap();
        await characterManager.SpawnCharacter();
    }
}
