using TMPro;
using UnityEngine;

public class CharacterStatusCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI characterHealthText;
    [SerializeField] CanvasGroup canvasGroup;

    Character character;
    public void Initialize(Character aCharacter)
    {
        character = aCharacter;
        UpdateStatus();
    }

    public void SetVisible(bool val)
    {
        canvasGroup.alpha = val ? 1 : 0;
    }

    public void UpdateStatus()
    {
        characterHealthText.text = $"Health: {character.CurrentHealth}/{character.MaxHealth}";
    }
}
