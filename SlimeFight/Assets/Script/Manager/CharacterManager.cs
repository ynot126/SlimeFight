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
    [SerializeField] float characterSelectionRadius = 0.75f;

    Dictionary<int, Character> characters = new Dictionary<int, Character>();
    
    int currentIdCounter = 1;
    
    GameData gameData = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;
    Character? hoveredCharacter;

    public void Initialize(GameData aGameData, MapManager aMapManager, InputManager aInputManager)
    {
        gameData = aGameData;
        mapManager = aMapManager;
        inputManager = aInputManager;
        inputManager.OnMousePositionUpdate += HandleMousePositionUpdate;
    }

    void OnDestroy()
    {
        inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;
    }

    void HandleMousePositionUpdate(Vector2 position)
    {
        Character? newHovered = TryGetCharacterAtPosition(position, -1, out var character) ? character : null;
        if (newHovered == hoveredCharacter) return;

        hoveredCharacter?.SetStatusCanvasVisible(false);
        hoveredCharacter = newHovered;
        hoveredCharacter?.SetStatusCanvasVisible(true);
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

    public IReadOnlyList<string> GetCharacterActions(int runTimeId)
        => characters[runTimeId].Actions;

    public void RefillMana(int runTimeId) => characters[runTimeId].RefillMana();

    public int GetCurrentMana(int runTimeId) => characters[runTimeId].CurrentMana;

    public int GetMaxMana(int runTimeId) => characters[runTimeId].MaxMana;

    public bool CanAffordMana(int runTimeId, int cost) => characters[runTimeId].CanAffordMana(cost);

    public bool TrySpendMana(int runTimeId, int cost) => characters[runTimeId].TrySpendMana(cost);
    void SpawnCharacter(CharacterData data, int runTimeId)
    {
        var randomPosition = mapManager.GetRandomPositionOnMap();
        var character = Instantiate(characterPrefab, randomPosition, Quaternion.identity, CharacterParent);
        character.Initialize(data , runTimeId);
        characters[runTimeId] = character;
        character.OnDeath += () =>
        {
            if (hoveredCharacter == character) hoveredCharacter = null;
            characters.Remove(runTimeId);
        };
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

    public bool TryGetCharacterAtPosition(Vector2 position, int excludeRunTimeId, out Character character)
    {
        character = null!;
        var closestDistance = characterSelectionRadius;
        foreach (var candidate in characters.Values)
        {
            if (candidate.RunTimeId == excludeRunTimeId) continue;
            var distance = Vector2.Distance(candidate.Position, position);
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            character = candidate;
        }

        return character != null;
    }

    public bool IsValidAttackTarget(int attackerRunTimeId, int targetRunTimeId)
    {
        if (!characters.TryGetValue(attackerRunTimeId, out var attacker)) return false;
        if (!characters.TryGetValue(targetRunTimeId, out var target)) return false;
        return attacker.Type != target.Type;
    }

    public void DealDamage(int targetRunTimeId, int damage)
    {
        if (!characters.TryGetValue(targetRunTimeId, out var target)) return;
        target.TakeDamage(damage);
    }

    public bool IsWithinRange(int sourceRunTimeId, int targetRunTimeId, float range)
    {
        if (!characters.TryGetValue(sourceRunTimeId, out var source)) return false;
        if (!characters.TryGetValue(targetRunTimeId, out var target)) return false;
        return Vector2.Distance(source.Position, target.Position) <= range;
    }

    public bool IsWithinRange(int sourceRunTimeId, Vector2 position, float range)
    {
        if (!characters.TryGetValue(sourceRunTimeId, out var source)) return false;
        return Vector2.Distance(source.Position, position) <= range;
    }
}
    