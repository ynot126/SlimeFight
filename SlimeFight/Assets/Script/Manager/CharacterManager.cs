#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
    [SerializeField] float statusCanvasHoverDelay = 0.2f;

    readonly Dictionary<int, Character> characters = new Dictionary<int, Character>();
    readonly Dictionary<int, CharacterStatusCanvas> statusCanvases = new Dictionary<int, CharacterStatusCanvas>();
    readonly Dictionary<int, List<string>> characterActions = new Dictionary<int, List<string>>();
    
    int currentIdCounter = 1;
    
    GameData gameData = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;
    Camera mainCamera = null!;
    Character? hoveredCharacter;
    CharacterStatusCanvas? hoveredStatusCanvas;
    CancellationTokenSource? hoverDelayCts;

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
        CancelHoverDelay();
        inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;
    }

    void HandleMousePositionUpdate(Vector3 position)
    {
        var newHovered = TryGetCharacterAtPosition(position, -1, out var character) ? character : null;
        if (newHovered == hoveredCharacter) return;

        CancelHoverDelay();
        hoveredStatusCanvas?.SetVisible(false);
        hoveredStatusCanvas = null;
        hoveredCharacter = newHovered;
        if (hoveredCharacter == null) return;

        ShowStatusCanvasAfterHoverDelay(hoveredCharacter).Forget();
    }

    void CancelHoverDelay()
    {
        hoverDelayCts?.Cancel();
        hoverDelayCts?.Dispose();
        hoverDelayCts = null;
    }

    async UniTaskVoid ShowStatusCanvasAfterHoverDelay(Character character)
    {
        hoverDelayCts = new CancellationTokenSource();
        var token = hoverDelayCts.Token;
        try
        {
            await UniTask.Delay((int)(statusCanvasHoverDelay * 1000f), cancellationToken: token);
            if (hoveredCharacter != character) return;
            if (!statusCanvases.TryGetValue(character.RunTimeId, out var canvas)) return;

            hoveredStatusCanvas = canvas;
            hoveredStatusCanvas.UpdateStatus(character);
            hoveredStatusCanvas.AnchorToWorldPosition(character.Position, mainCamera);
            hoveredStatusCanvas.SetVisible(true);
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    public async UniTask SpawnCharacter()
    {
        await UniTask.Yield();
        foreach (var data in gameData.playerCharacters)
            SpawnCharacter(data , currentIdCounter++);
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
        => characterActions[runTimeId];

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
        character.Initialize(data.stat, data.type, runTimeId);
        characters[runTimeId] = character;
        characterActions[runTimeId] = new List<string>(data.actionIds);

        var canvasRect = (RectTransform)characterStatusCanvas.transform;
        var statusCanvas = Instantiate(characterStatusCanvasPrefab, canvasRect);
        statusCanvas.Initialize(canvasRect);
        statusCanvases[runTimeId] = statusCanvas;

        character.OnDeath += () =>
        {
            if (statusCanvases.TryGetValue(runTimeId, out var canvas))
            {
                if (hoveredCharacter?.RunTimeId == runTimeId)
                {
                    CancelHoverDelay();
                    hoveredCharacter = null;
                }
                if (hoveredStatusCanvas == canvas)
                    hoveredStatusCanvas = null;
                Destroy(canvas.gameObject);
                statusCanvases.Remove(runTimeId);
            }
            characters.Remove(runTimeId);
            characterActions.Remove(runTimeId);
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
    