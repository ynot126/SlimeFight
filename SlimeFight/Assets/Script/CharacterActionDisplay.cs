#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class CharacterActionDisplay : MonoBehaviour
{
    const float indicatorY = 0.01f;
    const float pathY = 0.02f;

    [SerializeField] SpriteRenderer spriteRenderer = null!;
    [SerializeField] SpriteRenderer rangeSpriteRenderer = null!;
    [SerializeField] LineRenderer movePathLineRenderer = null!;

    public void Initialize()
    {
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
    }

    public void SetActionRangeIndicator(Vector3 centerPosition, float range, bool visible)
    {
        SetActionRangeIndicatorVisible(visible);
        if (!visible) return;

        var spriteDiameter = rangeSpriteRenderer.sprite.bounds.size.x;
        var scale = range * 2f / spriteDiameter;
        rangeSpriteRenderer.transform.position = new Vector3(centerPosition.x, indicatorY, centerPosition.z);
        rangeSpriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
