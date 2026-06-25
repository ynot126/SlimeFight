#nullable enable
using UnityEngine;

public class MapManager : MonoBehaviour
{
    GameData gameData = null!;
    public void Initialize(GameData aGameData)
    {
        gameData = aGameData;
    }
}