#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] Transform CharacterParent = null!;
    [SerializeField] Character characterPrefab = null!;
    [SerializeField] CharacterStatusCanvas characterStatusCanvasPrefab = null!;
    [SerializeField] Canvas characterStatusCanvas = null!;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float characterSelectionRadius = 0.75f;
    [SerializeField] float statusCanvasHoverDelay = 0.2f;

    #endregion

    #region Dependencies

    GameData gameData = null!;
    MapManager mapManager = null!;
    InputManager inputManager = null!;
    Camera mainCamera = null!;

    #endregion

    #region Runtime State

    readonly Dictionary<int, Character> characters = new();
    readonly Dictionary<int, CharacterStatusCanvas> statusCanvases = new();
    readonly Dictionary<int, List<string>> characterActions = new();
    readonly Dictionary<int, BotData> botDataByRunTimeId = new();
    int currentIdCounter = 1;
    Character? hoveredCharacter;
    CharacterStatusCanvas? hoveredStatusCanvas;
    CancellationTokenSource? hoverDelayCts;

    #endregion

    #region Lifecycle

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

    #endregion

    #region Spawning

    public async UniTask SpawnCharacter()
    {
        await UniTask.Yield();
        foreach (var data in gameData.playerCharacters)
            SpawnCharacter(data, currentIdCounter++);
    }

    public async UniTask SpawnEnemy()
    {
        await UniTask.Yield();
        var botData = BotLibrary.GetBot("Testing");
        SpawnEnemy(botData, currentIdCounter++);
    }

    void SpawnCharacter(CharacterData data, int runTimeId)
        => SpawnEntity(data.stat, CharacterType.Player, runTimeId, data.actionIds);

    void SpawnEnemy(BotData data, int runTimeId)
    {
        SpawnEntity(data.stat, CharacterType.Enemy, runTimeId, data.actionIds);
        botDataByRunTimeId[runTimeId] = data;
    }

    void SpawnEntity(EntityStat stat, CharacterType type, int runTimeId, IReadOnlyList<string> actionIds)
    {
        var mapPosition = mapManager.GetRandomPositionOnMap();
        var spawnPosition = new Vector3(mapPosition.x, 0f, mapPosition.z);
        var character = Instantiate(characterPrefab, CharacterParent);
        character.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        character.Initialize(stat, type, runTimeId);
        characters[runTimeId] = character;
        characterActions[runTimeId] = new List<string>(actionIds);

        var canvasRect = (RectTransform)characterStatusCanvas.transform;
        var statusCanvas = Instantiate(characterStatusCanvasPrefab, canvasRect);
        statusCanvas.Initialize(canvasRect);
        statusCanvases[runTimeId] = statusCanvas;

        character.OnDeath += () => HandleCharacterDeath(runTimeId, statusCanvas);
    }

    void HandleCharacterDeath(int runTimeId, CharacterStatusCanvas statusCanvas)
    {
        if (hoveredCharacter?.RunTimeId == runTimeId)
        {
            CancelHoverDelay();
            hoveredCharacter = null;
        }

        if (hoveredStatusCanvas == statusCanvas)
            hoveredStatusCanvas = null;

        Destroy(statusCanvas.gameObject);
        statusCanvases.Remove(runTimeId);
        characters.Remove(runTimeId);
        characterActions.Remove(runTimeId);
        botDataByRunTimeId.Remove(runTimeId);
    }

    #endregion

    #region Hover & Status Canvas

    void HandleMousePositionUpdate(Vector3 position)
    {
        Character? newHovered = null;
        if (TryGetCharacterAtPosition(position, -1, out var hoveredRunTimeId) && TryGetCharacter(hoveredRunTimeId, out var character))
            newHovered = character;
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

    #endregion

    #region Turn & Actions

    public List<int> GetMovementOrder()
    {
        return characters.Values
            .OrderByDescending(c => c.Speed)
            .Select(c => c.RunTimeId)
            .ToList();
    }

    public void SetCharacterReadyAction(bool val, int runTimeId)
        => characters[runTimeId].SetCharacterReadyAction(val);

    public IReadOnlyList<string> GetCharacterActions(int runTimeId)
        => characterActions[runTimeId];

    public CharacterType GetCharacterType(int runTimeId)
        => characters[runTimeId].Type;

    public bool TryGetBotData(int runTimeId, out BotData botData)
        => botDataByRunTimeId.TryGetValue(runTimeId, out botData);

    #endregion

    #region Mana

    public void RefillMana(int runTimeId) => characters[runTimeId].RefillMana();

    public int GetCurrentMana(int runTimeId) => characters[runTimeId].CurrentMana;

    public int GetMaxMana(int runTimeId) => characters[runTimeId].MaxMana;

    public bool CanAffordMana(int runTimeId, int cost) => characters[runTimeId].CanAffordMana(cost);

    public bool TrySpendMana(int runTimeId, int cost) => characters[runTimeId].TrySpendMana(cost);

    #endregion

    #region Movement

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

    #endregion

    #region Targeting & Combat

    public bool TryGetCharacterAtPosition(Vector3 position, int excludeRunTimeId, out int runTimeId)
    {
        runTimeId = 0;
        var closestDistance = characterSelectionRadius;
        foreach (var candidate in characters.Values)
        {
            if (candidate.RunTimeId == excludeRunTimeId) continue;
            var distance = Vector3.Distance(candidate.Position, position);
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            runTimeId = candidate.RunTimeId;
        }

        return runTimeId != 0;
    }

    public bool TryGetCharacter(int runTimeId, out Character character)
        => characters.TryGetValue(runTimeId, out character);

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

    public bool IsMovementPathBlocked(int sourceRunTimeId, Vector3 targetPosition)
    {
        if (!characters.TryGetValue(sourceRunTimeId, out var source)) return true;

        var sourcePosition = ToGroundPosition(source.Position);
        var destination = ToGroundPosition(targetPosition);
        var path = destination - sourcePosition;
        var pathLengthSqr = path.sqrMagnitude;

        foreach (var candidate in characters.Values)
        {
            if (candidate.RunTimeId == sourceRunTimeId) continue;
            var candidatePosition = ToGroundPosition(candidate.Position);
            if (pathLengthSqr <= Mathf.Epsilon)
            {
                if (Vector3.Distance(candidatePosition, destination) < characterSelectionRadius)
                    return true;
                continue;
            }

            var t = Vector3.Dot(candidatePosition - sourcePosition, path) / pathLengthSqr;
            t = Mathf.Clamp01(t);
            var closestPoint = sourcePosition + path * t;
            if (Vector3.Distance(candidatePosition, closestPoint) < characterSelectionRadius)
                return true;
        }

        return false;
    }

    Vector3 ToGroundPosition(Vector3 position)
        => new(position.x, 0f, position.z);

    public bool TryGetClosestValidAttackTarget(int attackerRunTimeId, float range, out Character target)
    {
        target = null!;
        if (!characters.TryGetValue(attackerRunTimeId, out var attacker)) return false;

        var closestDistance = float.MaxValue;
        foreach (var candidate in characters.Values)
        {
            if (!IsValidAttackTarget(attackerRunTimeId, candidate.RunTimeId)) continue;

            var distance = Vector3.Distance(attacker.Position, candidate.Position);
            if (distance > range || distance >= closestDistance) continue;

            closestDistance = distance;
            target = candidate;
        }

        return target != null;
    }

    public bool TryGetClosestOpponent(int runTimeId, out Character target)
    {
        target = null!;
        if (!characters.TryGetValue(runTimeId, out var character)) return false;

        var closestDistance = float.MaxValue;
        foreach (var candidate in characters.Values)
        {
            if (!IsValidAttackTarget(runTimeId, candidate.RunTimeId)) continue;

            var distance = Vector3.Distance(character.Position, candidate.Position);
            if (distance >= closestDistance) continue;

            closestDistance = distance;
            target = candidate;
        }

        return target != null;
    }

    #endregion
}
