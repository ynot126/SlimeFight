#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] Character characterPrefab = null!;
    
    List<Character> playerCharacters = new List<Character>();
    List<Character> enemyCharacters = new List<Character>();
    
    GameData gameData = null!;
    public void Initialize(GameData aGameData)
    {
        gameData = aGameData;
    }
}
