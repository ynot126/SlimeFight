#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameView : BaseView
{
    [Header("Character Action")]
    [SerializeField] CanvasGroup characterActionGroup= null!;
    [SerializeField] Button moveButton = null!;
    
    [Header("Other UI")]
    [SerializeField] Button endTurnButton = null!;
    public event Action? OnMoveButtonPressed;
    public event Action? OnEndTurnButtonPressed;
    public void Initialize()
    {
        moveButton.onClick.AddListener(() => OnMoveButtonPressed?.Invoke());
        endTurnButton.onClick.AddListener(() => OnEndTurnButtonPressed?.Invoke());
    }

    public void SetShowCharacterActionOption(bool val)
    {
        characterActionGroup.alpha = val ? 1 : 0;
    }
}
