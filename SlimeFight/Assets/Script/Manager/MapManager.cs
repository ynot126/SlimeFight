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

    public Vector3 GetRandomPositionOnMap()
    {
        var x = Random.Range(-5f, 5f);
        var z = Random.Range(-5f, 5f);
        return new Vector3(x,0,z);
    }

    public bool IsPositionOnMap(Vector3 position)
    {
        return position.x is > -5 and < 5 && position.z is > -5 and < 5;
    }
}