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
    readonly Dictionary<int, HexCoord> characterHexes = new();
    readonly Dictionary<int, CharacterStatusCanvas> statusCanvases = new();
    readonly Dictionary<int, List<string>> characterActions = new();
    readonly Dictionary<int, BotData> botDataByRunTimeId = new();
    int currentIdCounter = 1;
    Character? hoveredCharacter;
    CharacterStatusCanvas? hoveredStatusCanvas;
    CancellationTokenSource? hoverDelayCts;
    bool isActionExecuting;

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
        if (!mapManager.TryGetRandomEmptyHex(out var spawnHex)) return;

        var spawnPosition = mapManager.HexToWorld(spawnHex);
        mapManager.TrySetOccupant(spawnHex, runTimeId);
        characterHexes[runTimeId] = spawnHex;
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
        mapManager.ClearOccupant(runTimeId);
        statusCanvases.Remove(runTimeId);
        characters.Remove(runTimeId);
        characterHexes.Remove(runTimeId);
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
        if (hoveredCharacter == null || isActionExecuting) return;

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
            if (isActionExecuting || hoveredCharacter != character) return;
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

    public void SetActionExecuting(bool val)
    {
        if (isActionExecuting == val) return;

        isActionExecuting = val;
        CancelHoverDelay();
        hoveredStatusCanvas?.SetVisible(false);
        hoveredStatusCanvas = null;

        if (!isActionExecuting && hoveredCharacter != null)
            ShowStatusCanvasAfterHoverDelay(hoveredCharacter).Forget();
    }

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
        if (!mapManager.TryWorldToHex(position, out var destination)) return;
        await CharacterMoveToHex(runTimeId, destination);
    }

    public async UniTask CharacterMoveToHex(int runTimeId, HexCoord destination)
    {
        if (!characters.TryGetValue(runTimeId, out var character)) return;
        if (!characterHexes.TryGetValue(runTimeId, out var startHex)) return;
        if (!mapManager.IsHexOnMap(destination)) return;
        if (mapManager.IsHexOccupied(destination, runTimeId)) return;
        if (!mapManager.TryFindPath(startHex, destination, runTimeId, out var path)) return;
        if (path.Count <= 1) return;

        mapManager.ClearOccupant(startHex, runTimeId);
        if (!mapManager.TrySetOccupant(destination, runTimeId))
        {
            mapManager.TrySetOccupant(startHex, runTimeId);
            return;
        }

        characterHexes[runTimeId] = destination;
        var characterTransform = character.transform;
        for (var i = 1; i < path.Count; i++)
        {
            var end = mapManager.HexToWorld(path[i]);
            var distance = Vector3.Distance(characterTransform.position, end);
            if (distance <= Mathf.Epsilon) continue;

            var duration = distance / moveSpeed;
            await characterTransform.DOMove(end, duration).SetEase(Ease.Linear).ToUniTask();
        }

        characterTransform.position = mapManager.HexToWorld(destination);
    }

    #endregion

    #region Targeting & Combat

    public async UniTask CharacterAttackAnimation(int runTimeId)
    {
        if (!characters.TryGetValue(runTimeId, out var character)) return;
        await character.AttackAnimation();
    }

    public bool TryGetCharacterAtPosition(Vector3 position, int excludeRunTimeId, out int runTimeId)
    {
        runTimeId = 0;
        if (!mapManager.TryWorldToHex(position, out var hex)) return false;
        if (!mapManager.TryGetOccupant(hex, out var occupantRunTimeId)) return false;
        if (occupantRunTimeId == excludeRunTimeId) return false;

        runTimeId = occupantRunTimeId;
        return true;
    }

    public bool TryGetCharacter(int runTimeId, out Character character)
        => characters.TryGetValue(runTimeId, out character);

    public bool TryGetCharacterHex(int runTimeId, out HexCoord hex)
        => characterHexes.TryGetValue(runTimeId, out hex);

    public bool IsValidAttackTarget(int attackerRunTimeId, int targetRunTimeId)
    {
        if (!characters.TryGetValue(attackerRunTimeId, out var attacker)) return false;
        if (!characters.TryGetValue(targetRunTimeId, out var target)) return false;
        return attacker.Type != target.Type;
    }

    public async UniTask DealDamage(int targetRunTimeId, int damage)
    {
        if (!characters.TryGetValue(targetRunTimeId, out var target)) return;
        await target.DamageAnimation();
        target.TakeDamage(damage);
    }

    public bool IsWithinRange(int sourceRunTimeId, int targetRunTimeId, float range)
    {
        if (!characterHexes.TryGetValue(sourceRunTimeId, out var sourceHex)) return false;
        if (!characterHexes.TryGetValue(targetRunTimeId, out var targetHex)) return false;
        return mapManager.GetHexDistance(sourceHex, targetHex) <= Mathf.RoundToInt(range);
    }

    public bool IsWithinRange(int sourceRunTimeId, Vector3 position, float range)
    {
        if (!characterHexes.TryGetValue(sourceRunTimeId, out var sourceHex)) return false;
        if (!mapManager.TryWorldToHex(position, out var targetHex)) return false;
        return mapManager.GetHexDistance(sourceHex, targetHex) <= Mathf.RoundToInt(range);
    }

    public bool IsMovementPathBlocked(int sourceRunTimeId, Vector3 targetPosition)
    {
        if (!characterHexes.TryGetValue(sourceRunTimeId, out var sourceHex)) return true;
        if (!mapManager.TryWorldToHex(targetPosition, out var destination)) return true;
        if (mapManager.IsHexOccupied(destination, sourceRunTimeId)) return true;
        return !mapManager.TryFindPath(sourceHex, destination, sourceRunTimeId, out _);
    }

    public bool TryGetPathToPosition(int sourceRunTimeId, Vector3 targetPosition, out List<Vector3> pathPositions)
    {
        pathPositions = new List<Vector3>();
        if (!characterHexes.TryGetValue(sourceRunTimeId, out var sourceHex)) return false;
        if (!mapManager.TryWorldToHex(targetPosition, out var destination)) return false;
        if (!mapManager.TryFindPath(sourceHex, destination, sourceRunTimeId, out var path)) return false;

        foreach (var hex in path)
            pathPositions.Add(mapManager.HexToWorld(hex));
        return true;
    }

    public Dictionary<HexCoord, int> GetReachableHexes(int sourceRunTimeId, int range)
    {
        if (!characterHexes.TryGetValue(sourceRunTimeId, out var sourceHex))
            return new Dictionary<HexCoord, int>();

        return mapManager.GetReachableHexes(sourceHex, range, sourceRunTimeId);
    }

    public bool TryGetClosestValidAttackTarget(int attackerRunTimeId, float range, out Character target)
    {
        target = null!;
        if (!characterHexes.TryGetValue(attackerRunTimeId, out var attackerHex)) return false;

        var closestDistance = float.MaxValue;
        foreach (var candidate in characters.Values)
        {
            if (!IsValidAttackTarget(attackerRunTimeId, candidate.RunTimeId)) continue;
            if (!characterHexes.TryGetValue(candidate.RunTimeId, out var candidateHex)) continue;

            var distance = mapManager.GetHexDistance(attackerHex, candidateHex);
            if (distance > range || distance >= closestDistance) continue;

            closestDistance = distance;
            target = candidate;
        }

        return target != null;
    }

    public bool TryGetClosestOpponent(int runTimeId, out Character target)
    {
        target = null!;
        if (!characterHexes.TryGetValue(runTimeId, out var characterHex)) return false;

        var closestDistance = float.MaxValue;
        foreach (var candidate in characters.Values)
        {
            if (!IsValidAttackTarget(runTimeId, candidate.RunTimeId)) continue;
            if (!characterHexes.TryGetValue(candidate.RunTimeId, out var candidateHex)) continue;

            var distance = mapManager.GetHexDistance(characterHex, candidateHex);
            if (distance >= closestDistance) continue;

            closestDistance = distance;
            target = candidate;
        }

        return target != null;
    }

    #endregion
}
