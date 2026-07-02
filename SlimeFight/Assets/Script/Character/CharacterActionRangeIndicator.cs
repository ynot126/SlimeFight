#nullable enable
using UnityEngine;

public class CharacterActionRangeIndicator : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer = null!;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetRange(float range)
    {
        var spriteDiameter = spriteRenderer.sprite.bounds.size.x;
        var scale = range * 2f / spriteDiameter;
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
