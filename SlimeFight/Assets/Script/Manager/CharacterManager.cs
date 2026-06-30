#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField] Transform CharacterParent = null!;
    [SerializeField] Character characterPrefab = null!;
    [SerializeField] float moveSpeed = 5f;

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

    public async UniTask CharacterMoveToPosition(int runTimeId, Vector2 position)
    {
        var characterTransform = characters[runTimeId].transform;
        var start = characterTransform.position;
        var end = new Vector3(position.x, position.y, start.z);
        var distance = Vector2.Distance(start, end);
        if (distance <= Mathf.Epsilon) return;

        var duration = distance / moveSpeed;
        await characterTransform.DOMove(end, duration).SetEase(Ease.Linear).ToUniTask();
    }
}
    