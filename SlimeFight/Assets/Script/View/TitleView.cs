#nullable enable
using System;
using UnityEngine;
using UnityEngine.UI;

public class TitleView : BaseView
{
   [SerializeField] Button startGameButton= null!;
   public event Action? OnStartButtonPressed;

   public void Initialize()
   {
      startGameButton.onClick.AddListener(()=> OnStartButtonPressed?.Invoke());
   }
}
