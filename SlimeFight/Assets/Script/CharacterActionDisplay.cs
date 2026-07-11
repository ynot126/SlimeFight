#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class CharacterActionDisplay : MonoBehaviour
{
    const float indicatorY = 0.01f;
    const float pathY = 0.02f;
    const float hexIndicatorLineWidth = 0.04f;

    [SerializeField] SpriteRenderer spriteRenderer = null!;
    [SerializeField] SpriteRenderer rangeSpriteRenderer = null!;
    [SerializeField] LineRenderer movePathLineRenderer = null!;

    readonly List<LineRenderer> hexRangeIndicators = new();
    Material? lineMaterial;

    public void Initialize()
    {
        lineMaterial = new Material(Shader.Find("Sprites/Default"));
        Cleanup();
    }

    public void Cleanup()
    {
        SetVisible(false);
        SetActionRangeIndicatorVisible(false);
        SetMovePathIndicatorVisible(false);
    }

    public void SetPosition(Vector3 position)
    {
        spriteRenderer.transform.position = new Vector3(position.x, indicatorY, position.z);
    }

    public void SetVisible(bool visible)
    {
        spriteRenderer.gameObject.SetActive(visible);
    }

    public void SetValidTargetVisual(bool val)
    {
        spriteRenderer.color = val ? Color.blue : Color.green;
    }

    public void SetMovePathIndicatorVisible(bool visible)
    {
        movePathLineRenderer.gameObject.SetActive(visible);
    }

    public void SetMovePathIndicator(Vector3 startPosition, Vector3 endPosition, bool valid, bool visible)
        => SetMovePathIndicator(new List<Vector3> { startPosition, endPosition }, valid, visible);

    public void SetMovePathIndicator(IReadOnlyList<Vector3> pathPositions, bool valid, bool visible)
    {
        SetMovePathIndicatorVisible(visible);
        if (!visible || pathPositions.Count < 2) return;

        var lineColor = valid ? Color.blue : Color.green;
        movePathLineRenderer.positionCount = pathPositions.Count;
        movePathLineRenderer.startColor = lineColor;
        movePathLineRenderer.endColor = lineColor;
        for (var i = 0; i < pathPositions.Count; i++)
        {
            var position = pathPositions[i];
            movePathLineRenderer.SetPosition(i, new Vector3(position.x, pathY, position.z));
        }
    }

    public void SetActionRangeIndicatorVisible(bool visible)
    {
        rangeSpriteRenderer.gameObject.SetActive(visible);
        foreach (var indicator in hexRangeIndicators)
            indicator.gameObject.SetActive(visible);
    }

    public void SetActionRangeIndicator(Vector3 centerPosition, float range, bool visible)
    {
        SetActionRangeIndicatorVisible(visible);
        if (!visible) return;

        foreach (var indicator in hexRangeIndicators)
            indicator.gameObject.SetActive(false);

        var spriteDiameter = rangeSpriteRenderer.sprite.bounds.size.x;
        var scale = range * 2f / spriteDiameter;
        rangeSpriteRenderer.transform.position = new Vector3(centerPosition.x, indicatorY, centerPosition.z);
        rangeSpriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void SetHexRangeIndicator(IReadOnlyList<Vector3> hexCenters, float hexSize, bool visible)
    {
        rangeSpriteRenderer.gameObject.SetActive(false);
        EnsureHexRangeIndicatorCount(hexCenters.Count);

        for (var i = 0; i < hexRangeIndicators.Count; i++)
        {
            var indicator = hexRangeIndicators[i];
            var isActive = visible && i < hexCenters.Count;
            indicator.gameObject.SetActive(isActive);
            if (!isActive) continue;

            indicator.positionCount = 7;
            indicator.startColor = Color.blue;
            indicator.endColor = Color.blue;
            indicator.SetPositions(HexGridUtility.GetClosedCornerPositions(hexCenters[i], hexSize, indicatorY));
        }
    }

    void EnsureHexRangeIndicatorCount(int count)
    {
        while (hexRangeIndicators.Count < count)
            hexRangeIndicators.Add(CreateHexRangeIndicator());
    }

    LineRenderer CreateHexRangeIndicator()
    {
        var indicator = new GameObject("HexRangeIndicator");
        indicator.transform.SetParent(transform, false);

        var lineRenderer = indicator.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.positionCount = 7;
        lineRenderer.startWidth = hexIndicatorLineWidth;
        lineRenderer.endWidth = hexIndicatorLineWidth;
        lineRenderer.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lineRenderer.gameObject.SetActive(false);
        return lineRenderer;
    }
}
