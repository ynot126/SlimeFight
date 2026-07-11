#nullable enable
using UnityEngine;

public enum HexGridState
{
    Normal,
    Range,
    ValidHover,
    InvalidHover,
}

public class HexGrid : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer = null!;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color rangeColor = Color.blue;
    [SerializeField] Color validHoverColor = Color.cyan;
    [SerializeField] Color invalidHoverColor = Color.red;

    public HexCoord Coord { get; private set; }
    public HexGridState State { get; private set; }

    public void Initialize(HexCoord coord, float hexSize)
    {
        Coord = coord;

        var spriteWidth = spriteRenderer.sprite != null
            ? spriteRenderer.sprite.bounds.size.x
            : 1f;
        transform.localScale = Vector3.one * (hexSize * 2f / spriteWidth);

        SetState(HexGridState.Normal);
    }

    public void SetState(HexGridState state)
    {
        State = state;
        spriteRenderer.color = state switch
        {
            HexGridState.Range => rangeColor,
            HexGridState.ValidHover => validHoverColor,
            HexGridState.InvalidHover => invalidHoverColor,
            _ => normalColor,
        };
    }
}
