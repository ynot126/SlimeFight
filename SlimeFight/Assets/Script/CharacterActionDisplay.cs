#nullable enable
using UnityEngine;

public class CharacterActionDisplay : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer = null!;
    [SerializeField] SpriteRenderer rangeSpriteRenderer = null!;

    void Awake()
    {
        rangeSpriteRenderer.gameObject.SetActive(false);
        spriteRenderer.gameObject.SetActive(false);
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

    public void SetActionRangeIndicator(Vector3 centerPosition, BaseTargetSelectStrategy targetStrategy)
    {
        if (targetStrategy is not MouseTargetSelectStrategy mouseStrategy)
        {
            SetActionRangeIndicatorVisible(false);
            return;
        }

        SetActionRangeIndicator(centerPosition, mouseStrategy.Range, true);
    }

    public void SetActionRangeIndicatorVisible(bool visible)
    {
        rangeSpriteRenderer.gameObject.SetActive(visible);
    }

    void SetActionRangeIndicator(Vector3 centerPosition, float range, bool visible)
    {
        SetActionRangeIndicatorVisible(visible);
        if (!visible) return;

        var spriteDiameter = rangeSpriteRenderer.sprite.bounds.size.x;
        var scale = range * 2f / spriteDiameter;
        rangeSpriteRenderer.transform.position = new Vector3(centerPosition.x, 0.01f, centerPosition.z);
        rangeSpriteRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
