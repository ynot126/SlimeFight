#nullable enable
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : CustomButton
{
    [SerializeField] TextMeshProUGUI actionName = null!;
    [SerializeField] Image buttonImage = null!;
    public void Initialize(CharacterAction characterAction)
    {
        actionName.text = $"{characterAction.ActionName} ({characterAction.ManaCost})";
    }
    
    public void SetButtonSelectState(bool val)
    {
        transform.DOScale(val?1.2f:1f, 0.2f);
    }

    public void SetButtonSelectable(bool val)
    {
        buttonImage.color = val? Color.white: Color.gray;
    }
}