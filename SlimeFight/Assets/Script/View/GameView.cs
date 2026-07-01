#nullable enable
using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : BaseView
{
    [Header("Character Action")]
    [SerializeField] CanvasGroup characterActionGroup= null!;
    [SerializeField] Button moveButton = null!;
    [SerializeField] Button attackButton = null!;
    
    [Header("Other UI")]
    [SerializeField] Button endTurnButton = null!;
    [SerializeField] TextMeshProUGUI roundText = null!;
    
    public event Action? OnMoveButtonPressed;
    public event Action? OnAttackButtonPressed;
    public event Action? OnEndTurnButtonPressed;
    public void Initialize()
    {
        moveButton.onClick.AddListener(() => OnMoveButtonPressed?.Invoke());
        attackButton.onClick.AddListener(() => OnAttackButtonPressed?.Invoke());
        
        endTurnButton.onClick.AddListener(() => OnEndTurnButtonPressed?.Invoke());
    }

    public void SetShowCharacterActionOption(bool val)
    {
        characterActionGroup.alpha = val ? 1 : 0;
    }

    public void SetMoveButtonSelectedState(bool val)
    {
        moveButton.transform.DOScale(val?1.2f:1f, 0.2f);
    }

    public void SetAttackButtonSelectState(bool val)
    {
        attackButton.transform.DOScale(val?1.2f:1f, 0.2f);
    }

    public void SetRoundText(int round)
    {
        roundText.text = $"Round {round}";
    }
}
