using TMPro;
using UnityEngine;

public class CharacterStatusCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI characterHealthText = null!;
    [SerializeField] CanvasGroup canvasGroup = null!;
    [SerializeField] float worldAnchorHeight = 0.5f;

    public void Initialize()
    {
        transform.localPosition = new Vector3(0f, worldAnchorHeight, 0f);
        SetVisible(false);
    }

    public void SetVisible(bool val)
    {
        canvasGroup.alpha = val ? 1 : 0;
    }

    public void UpdateStatus(int currentHealth, int maxHealth)
    {
        characterHealthText.text = $"Health: {currentHealth}/{maxHealth}";
    }
}
