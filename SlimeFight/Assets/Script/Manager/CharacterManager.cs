#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] Transform CharacterParent = null!;
    [SerializeField] Character characterPrefab = null!;
    
    List<Character> playerCharacters = new List<Character>();
    List<Character> enemyCharacters = new List<Character>();
    
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
        foreach (var data in gameData.playerCharacters)
        {
            var character = SpawnCharacter(data);
            playerCharacters.Add(character);
        }

        foreach (var data in gameData.enemyCharacters)
        {
            var character = SpawnCharacter(data);
            enemyCharacters.Add(character);
        }
    }

    Character SpawnCharacter(CharacterData data)
    {
        var randomPosition = mapManager.GetRandomPositionOnMap();
        var character = Instantiate(characterPrefab, randomPosition, Quaternion.identity, CharacterParent);
        character.Initialize(data);
        return character;
    }
}
