#nullable enable
using UnityEngine;

public class CharacterActionDisplay : MonoBehaviour
{
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
        spriteRenderer.transform.position = new Vector3(position.x, 0.01f, position.z);
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
    {
        SetMovePathIndicatorVisible(visible);
        if (!visible) return;

        var lineColor = valid ? Color.blue : Color.green;
        movePathLineRenderer.positionCount = 2;
        movePathLineRenderer.startColor = lineColor;
        movePathLineRenderer.endColor = lineColor;
        movePathLineRenderer.SetPosition(0, new Vector3(startPosition.x, 0.02f, startPosition.z));
        movePathLineRenderer.SetPosition(1, new Vector3(endPosition.x, 0.02f, endPosition.z));
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
        rangeSpriteRenderer.transform.position = new Vector3(centerPosition.x, 0.01f, centerPosition.z);
        rangeSpriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
