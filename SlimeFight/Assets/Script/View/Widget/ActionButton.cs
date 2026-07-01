#nullable enable
using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI actionName = null!;
    [SerializeField] Button button = null!;
    
    public event Action? OnActionButtonPressed;

    public void Initialize(CharacterAction characterAction)
    {
        actionName.text = $"{characterAction.ActionName} ({characterAction.ManaCost})";
        button.onClick.AddListener(()=>OnActionButtonPressed?.Invoke());
    }

    public void SetInteractable(bool val) => button.interactable = val;
    public void SetButtonSelectState(bool val)
    {
        transform.DOScale(val?1.2f:1f, 0.2f);
    }
}
