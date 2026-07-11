#nullable enable
using UnityEngine;

public enum HexGridState
{
    Normal,
    Range,
}

public class HexGrid : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer = null!;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color rangeColor = Color.blue;

    public HexCoord Coord { get; private set; }
    public HexGridState State { get; private set; }

    public void Initialize(HexCoord coord, float hexSize)
    {
        Coord = coord;

        var spriteHeight = spriteRenderer.sprite != null
            ? spriteRenderer.sprite.bounds.size.y
            : 1f;
        transform.localScale = Vector3.one * (hexSize * 2f / spriteHeight);

        SetState(HexGridState.Normal);
    }

    public void SetState(HexGridState state)
    {
        State = state;
        spriteRenderer.color = state == HexGridState.Range
            ? rangeColor
            : normalColor;
    }
}
