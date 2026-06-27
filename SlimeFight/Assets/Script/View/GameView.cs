#nullable enable
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameView : BaseView
{
    [Header("Character Action")]
    [SerializeField] CanvasGroup characterActionGroup= null!;
    [SerializeField] Button moveButton = null!;
    
    public event Action? OnMoveButtonPressed;
    public void Initialize()
    {
        moveButton.onClick.AddListener(() => OnMoveButtonPressed?.Invoke());
    }

    public void SetShowCharacterActionOption(bool val)
    {
        characterActionGroup.alpha = val ? 1 : 0;
    }
}
