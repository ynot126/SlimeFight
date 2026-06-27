#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] Transform CharacterParent = null!;
    [SerializeField] Character characterPrefab = null!;

    Dictionary<int, Character> characters = new Dictionary<int, Character>();
    
    int currentIdCounter = 1;
    
    GameData gameData = null!;
    MapManager mapManager = null!;
    public void Initialize(GameData aGameData, MapManager aMapManager)
    {
        gameData = aGameData;
        mapManager = aMapManager;
    }

    public async UniTask SpawnCharacter()
    {
        await UniTask.Yield();
        foreach (var data in gameData.playerCharacters) SpawnCharacter(data , currentIdCounter++);
        foreach (var data in gameData.enemyCharacters) SpawnCharacter(data, currentIdCounter++);
    }

    public List<int> GetMovementOrder()
    {
        return characters.Values
            .OrderByDescending(c => c.Speed)
            .Select(c => c.RunTimeId)
            .ToList();
    }

    public void SetCharacterReadyAction(bool val , int runTimeId)
    {
        characters[runTimeId].SetCharacterReadyAction(val);
    }
    void SpawnCharacter(CharacterData data, int runTimeId)
    {
        var randomPosition = mapManager.GetRandomPositionOnMap();
        var character = Instantiate(characterPrefab, randomPosition, Quaternion.identity, CharacterParent);
        character.Initialize(data , runTimeId);
        characters[runTimeId] = character;
        character.OnDeath += () => characters.Remove(runTimeId);
    }
}
    