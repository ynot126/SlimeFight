#nullable enable
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    GameData gameData = null!;
    public void Initialize(GameData aGameData)
    {
        gameData = aGameData;
    }

    public async UniTask CreateMap()
    {
        await UniTask.Yield();
    }

    public Vector2 GetRandomPositionOnMap()
    {
        var x = Random.Range(-5f, 5f);
        var y = Random.Range(-5f, 5f);
        return new Vector2(x, y);
    }

    public bool IsPositionOnMap(Vector2 position)
    {
        return position.x is > -5 and < 5 && position.y is > -5 and < 5;
    }
}