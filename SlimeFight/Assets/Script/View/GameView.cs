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
    [SerializeField] Transform actionButtonContainer = null!;
    [SerializeField] ActionButton actionButtonPrefab = null!;

    [Header("Other UI")]
    [SerializeField] Button endTurnButton = null!;
    [SerializeField] TextMeshProUGUI roundText = null!;

    readonly List<(ActionButton button, CharacterAction action)> spawnedActionButtons = new();

    public event Action? OnEndTurnButtonPressed;

    public void Initialize()
    {
        endTurnButton.onClick.AddListener(() => OnEndTurnButtonPressed?.Invoke());
    }

    public void SpawnActionButtons(IReadOnlyList<CharacterAction> actions, Action<CharacterAction> onActionSelected)
    {
        ClearActionButtons();
        foreach (var action in actions)
        {
            var actionButton = Instantiate(actionButtonPrefab, actionButtonContainer);
            actionButton.Initialize(action);
            actionButton.OnActionButtonPressed += () => onActionSelected(action);
            spawnedActionButtons.Add((actionButton, action));
        }
    }

    public void ClearActionButtons()
    {
        foreach (var (button, _) in spawnedActionButtons)
            Destroy(button.gameObject);
        spawnedActionButtons.Clear();
    }

    public void UpdateActionButtonSelection(CharacterAction? selectedAction)
    {
        foreach (var (button, action) in spawnedActionButtons)
            button.SetButtonSelectState(action == selectedAction);
    }

    public void SetShowCharacterActionOption(bool val)
    {
        characterActionGroup.alpha = val ? 1 : 0;
    }

    public void SetRoundText(int round)
    {
        roundText.text = $"Round {round}";
    }
}
