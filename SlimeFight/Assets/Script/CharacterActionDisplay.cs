#nullable enable
using UnityEngine;

public class CharacterActionDisplay : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer = null!;

    public void SetPosition(Vector3 position)
    {
        transform.position = new Vector3(position.x, 0.01f, position.z);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetValidTargetVisual(bool val)
    {
        spriteRenderer.color = val ? Color.blue : Color.green;
    }
}
