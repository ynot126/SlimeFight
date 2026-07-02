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
    [SerializeField] CharacterStatusCanvas characterStatusCanvasPrefab = null!;
    [SerializeField] Canvas characterStatusCanvas = null!;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float characterSelectionRadius = 0.75f;

    readonly Dictionary<int, Character> characters = new Dictionary<int, Character>();
    readonly Dictionary<int, CharacterStatusCanvas> statusCanvases = new Dictionary<int, CharacterStatusCanvas>();
    
    int currentIdCounter = 1;
    
    GameData gameData = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;
    Camera mainCamera = null!;
    Character? hoveredCharacter;
    CharacterStatusCanvas? hoveredStatusCanvas;

    public void Initialize(GameData aGameData, MapManager aMapManager, InputManager aInputManager, Camera aMainCamera)
    {
        gameData = aGameData;
        mapManager = aMapManager;
        inputManager = aInputManager;
        mainCamera = aMainCamera;
        inputManager.OnMousePositionUpdate += HandleMousePositionUpdate;
    }

    void OnDestroy()
    {
        inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;
    }

    void HandleMousePositionUpdate(Vector3 position)
    {
        var newHovered = TryGetCharacterAtPosition(position, -1, out var character) ? character : null;
        if (newHovered == hoveredCharacter) return;

        hoveredStatusCanvas?.SetVisible(false);
        hoveredCharacter = newHovered;
        hoveredStatusCanvas = hoveredCharacter != null && statusCanvases.TryGetValue(hoveredCharacter.RunTimeId, out var canvas)
            ? canvas
            : null;
        if (hoveredStatusCanvas == null) return;

        hoveredStatusCanvas.UpdateStatus(hoveredCharacter);
        hoveredStatusCanvas.AnchorToWorldPosition(hoveredCharacter.Position, mainCamera);
        hoveredStatusCanvas.SetVisible(true);
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

    public void SetActionRangeIndicator(int runTimeId, float range, bool visible)
    {
        characters[runTimeId].SetActionRangeIndicator(range, visible);
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
        var mapPosition = mapManager.GetRandomPositionOnMap();
        var spawnPosition = new Vector3(mapPosition.x, 0f, mapPosition.z);
        var character = Instantiate(characterPrefab, CharacterParent);
        character.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        character.Initialize(data , runTimeId);
        characters[runTimeId] = character;

        var canvasRect = (RectTransform)characterStatusCanvas.transform;
        var statusCanvas = Instantiate(characterStatusCanvasPrefab, canvasRect);
        statusCanvas.Initialize(canvasRect);
        statusCanvases[runTimeId] = statusCanvas;

        character.OnDeath += () =>
        {
            if (statusCanvases.TryGetValue(runTimeId, out var canvas))
            {
                if (hoveredStatusCanvas == canvas)
                {
                    hoveredCharacter = null;
                    hoveredStatusCanvas = null;
                }
                Destroy(canvas.gameObject);
                statusCanvases.Remove(runTimeId);
            }
            characters.Remove(runTimeId);
        };
    }

    public async UniTask CharacterMoveToPosition(int runTimeId, Vector3 position)
    {
        var characterTransform = characters[runTimeId].transform;
        var start = characterTransform.position;
        var end = new Vector3(position.x, 0f, position.z);
        var distance = Vector3.Distance(start, end);
        if (distance <= Mathf.Epsilon) return;

        var duration = distance / moveSpeed;
        await characterTransform.DOMove(end, duration).SetEase(Ease.Linear).ToUniTask();
    }

    public bool TryGetCharacterAtPosition(Vector3 position, int excludeRunTimeId, out Character character)
    {
        character = null!;
        var closestDistance = characterSelectionRadius;
        foreach (var candidate in characters.Values)
        {
            if (candidate.RunTimeId == excludeRunTimeId) continue;
            var distance = Vector3.Distance(candidate.Position, position);
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
        return Vector3.Distance(source.Position, target.Position) <= range;
    }

    public bool IsWithinRange(int sourceRunTimeId, Vector3 position, float range)
    {
        if (!characters.TryGetValue(sourceRunTimeId, out var source)) return false;
        return Vector3.Distance(source.Position, position) <= range;
    }
}
    