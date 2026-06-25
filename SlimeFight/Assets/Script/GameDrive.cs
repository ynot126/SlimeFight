#nullable enable
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
        characterManager.Initialize(gameData);
    }
}
