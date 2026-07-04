#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    [SerializeField] GameData testingGameData = null!;
    public GameData GetGameData()
    {
        return testingGameData;
    }
}

[Serializable]
public class GameData
{
    public List<CharacterData> playerCharacters = new List<CharacterData>();
}