#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    [SerializeField] int mapRadius = 4;
    [SerializeField] float hexSize = 1f;
    [SerializeField] HexGrid hexGridPrefab = null!;

    GameData gameData = null!;
    readonly HashSet<HexCoord> hexes = new();
    readonly Dictionary<HexCoord, int> occupants = new();
    readonly Dictionary<HexCoord, HexGrid> hexGridVisuals = new();
    readonly HashSet<HexCoord> rangeHexes = new();
    Bounds mapWorldBounds;

    public float HexSize => hexSize;
    public Bounds MapWorldBounds => mapWorldBounds;

    public void Initialize(GameData aGameData)
    {
        gameData = aGameData;
    }

    public async UniTask CreateMap()
    {
        ClearHexGridVisuals();
        hexes.Clear();
        occupants.Clear();
        GenerateHexes();
        CalculateMapWorldBounds();
        CreateHexGridVisuals();
        await UniTask.Yield();
    }

    public Vector3 GetRandomPositionOnMap()
    {
        return TryGetRandomEmptyHex(out var hex)
            ? HexToWorld(hex)
            : Vector3.zero;
    }

    public bool IsPositionOnMap(Vector3 position)
    {
        return TryWorldToHex(position, out _);
    }

    public bool TryWorldToHex(Vector3 position, out HexCoord hex)
    {
        hex = HexGridUtility.WorldToHex(position, hexSize);
        return IsHexOnMap(hex);
    }

    public Vector3 HexToWorld(HexCoord hex)
        => HexGridUtility.HexToWorld(hex, hexSize);

    public bool IsHexOnMap(HexCoord hex)
        => hexes.Contains(hex);

    public bool TryGetRandomEmptyHex(out HexCoord hex)
    {
        var emptyHexes = new List<HexCoord>();
        foreach (var candidate in hexes)
        {
            if (occupants.ContainsKey(candidate)) continue;
            emptyHexes.Add(candidate);
        }

        if (emptyHexes.Count <= 0)
        {
            hex = default;
            return false;
        }

        hex = emptyHexes[Random.Range(0, emptyHexes.Count)];
        return true;
    }

    public bool TrySetOccupant(HexCoord hex, int runTimeId)
    {
        if (!IsHexOnMap(hex)) return false;
        if (occupants.TryGetValue(hex, out var existingRunTimeId) && existingRunTimeId != runTimeId)
            return false;

        occupants[hex] = runTimeId;
        return true;
    }

    public void ClearOccupant(HexCoord hex, int runTimeId)
    {
        if (!occupants.TryGetValue(hex, out var existingRunTimeId)) return;
        if (existingRunTimeId != runTimeId) return;

        occupants.Remove(hex);
    }

    public void ClearOccupant(int runTimeId)
    {
        HexCoord? hexToClear = null;
        foreach (var pair in occupants)
        {
            if (pair.Value != runTimeId) continue;
            hexToClear = pair.Key;
            break;
        }

        if (hexToClear.HasValue)
            occupants.Remove(hexToClear.Value);
    }

    public bool TryGetOccupant(HexCoord hex, out int runTimeId)
        => occupants.TryGetValue(hex, out runTimeId);

    public bool IsHexOccupied(HexCoord hex, int ignoredRunTimeId = 0)
    {
        return occupants.TryGetValue(hex, out var runTimeId) && runTimeId != ignoredRunTimeId;
    }

    public List<HexCoord> GetHexesInRange(HexCoord center, int range)
    {
        var results = new List<HexCoord>();
        foreach (var hex in HexGridUtility.GetHexesInRange(center, range))
        {
            if (!IsHexOnMap(hex)) continue;
            results.Add(hex);
        }

        return results;
    }

    public Dictionary<HexCoord, int> GetReachableHexes(HexCoord start, int range, int movingRunTimeId)
    {
        return HexPathfinder.FindReachable(
            start,
            range,
            hexes,
            hex => !IsHexOccupied(hex, movingRunTimeId));
    }

    public bool TryFindPath(HexCoord start, HexCoord destination, int movingRunTimeId, out List<HexCoord> path)
    {
        return HexPathfinder.TryFindPath(
            start,
            destination,
            hexes,
            hex => !IsHexOccupied(hex, movingRunTimeId),
            out path);
    }

    public int GetHexDistance(HexCoord a, HexCoord b)
        => HexGridUtility.Distance(a, b);

    public void ShowRange(IEnumerable<HexCoord> range)
    {
        ClearRange();

        foreach (var hex in range)
        {
            if (!hexGridVisuals.TryGetValue(hex, out var hexGrid)) continue;

            hexGrid.SetState(HexGridState.Range);
            rangeHexes.Add(hex);
        }
    }

    public void ClearRange()
    {
        foreach (var hex in rangeHexes)
        {
            if (hexGridVisuals.TryGetValue(hex, out var hexGrid))
                hexGrid.SetState(HexGridState.Normal);
        }

        rangeHexes.Clear();
    }

    void GenerateHexes()
    {
        for (var q = -mapRadius; q <= mapRadius; q++)
        {
            var minR = Mathf.Max(-mapRadius, -q - mapRadius);
            var maxR = Mathf.Min(mapRadius, -q + mapRadius);
            for (var r = minR; r <= maxR; r++)
                hexes.Add(new HexCoord(q, r));
        }
    }

    void CalculateMapWorldBounds()
    {
        var hasBounds = false;
        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var hex in hexes)
        {
            var center = HexToWorld(hex);
            foreach (var corner in HexGridUtility.GetCornerPositions(center, hexSize, 0f))
            {
                if (!hasBounds)
                {
                    bounds = new Bounds(corner, Vector3.zero);
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(corner);
            }
        }

        mapWorldBounds = bounds;
    }

    void CreateHexGridVisuals()
    {
        foreach (var hex in hexes)
        {
            var hexGrid = Instantiate(
                hexGridPrefab,
                HexToWorld(hex),
                hexGridPrefab.transform.rotation,
                transform);
            hexGrid.name = $"HexGrid {hex.Q},{hex.R}";
            hexGrid.Initialize(hex, hexSize);
            hexGridVisuals.Add(hex, hexGrid);
        }
    }

    void ClearHexGridVisuals()
    {
        foreach (var hexGrid in hexGridVisuals.Values)
        {
            if (hexGrid != null)
                Destroy(hexGrid.gameObject);
        }

        hexGridVisuals.Clear();
        rangeHexes.Clear();
    }
}