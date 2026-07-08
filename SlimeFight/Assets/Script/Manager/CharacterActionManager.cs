#nullable enable

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CharacterActionManager : BaseCharacterActionManager
{
    bool isAwaitingActionConfirmation;
    CharacterAction? selectedAction;

    CancellationTokenSource? actionConfirmationCts;
    UniTaskCompletionSource? actionSelectionSource;
    System.Action? cancelCurrentTargetSelection;

    #region Turn Lifecycle

    protected override void BeginTurn(int runTimeId, GameView gameView)
    {
        base.BeginTurn(runTimeId, gameView);

        selectedAction = null;
        UpdateManaDisplay(runTimeId);

        ActiveGameView.SpawnActionButtons(AvailableActions);
        ActiveGameView.SetShowCharacterActionOption(true);
        ActiveGameView.OnActionSelected += SelectAction;
        ActiveGameView.OnEndTurnButtonPressed += HandleEndTurnButtonPressed;
        ActiveGameView.OnActionHover += HandleActionHover;
        ActiveGameView.OnActionHoverExit += HandleActionHoverExit;

        UpdateActionButtonSelection();
        UpdateActionButtonAffordability();
    }

    protected override void EndTurn()
    {
        ActiveGameView.OnActionSelected -= SelectAction;
        ActiveGameView.OnEndTurnButtonPressed -= HandleEndTurnButtonPressed;
        ActiveGameView.OnActionHover -= HandleActionHover;
        ActiveGameView.OnActionHoverExit -= HandleActionHoverExit;
        ActiveGameView.ClearActionButtons();
        ActiveGameView.ClearManaText();
        ActiveGameView.SetShowCharacterActionOption(false);

        selectedAction = null;
        base.EndTurn();
    }

    #endregion

    #region Planning Phase

    protected override async UniTask<CharacterAction?> GetNextAction()
    {
        await WaitForActionConfirmation();
        return IsEndTurnRequested ? null : selectedAction;
    }

    async UniTask WaitForActionConfirmation()
    {
        isAwaitingActionConfirmation = true;
        actionConfirmationCts = new CancellationTokenSource();

        try
        {
            while (!IsEndTurnRequested)
            {
                await WaitUntilActionSelected();
                if (IsEndTurnRequested) return;

                if (await TrySelectTargetsForSelectedAction(actionConfirmationCts.Token))
                    return;
            }
        }
        finally
        {
            isAwaitingActionConfirmation = false;
            actionSelectionSource?.TrySetCanceled();
            actionSelectionSource = null;
            cancelCurrentTargetSelection?.Invoke();
            cancelCurrentTargetSelection = null;
            actionConfirmationCts?.Cancel();
            actionConfirmationCts?.Dispose();
            actionConfirmationCts = null;
            HideActionRangeIndicator();
        }
    }

    async UniTask WaitUntilActionSelected()
    {
        while (selectedAction == null && !IsEndTurnRequested)
        {
            actionSelectionSource = new UniTaskCompletionSource();
            await actionSelectionSource.Task;
            actionSelectionSource = null;
        }
    }

    async UniTask<bool> TrySelectTargetsForSelectedAction(CancellationToken confirmationToken)
    {
        var action = selectedAction;
        if (action == null) return false;

        if (action.TargetStrategy is AutoTargetSelectStrategy)
            throw new System.NotImplementedException("AutoTargetSelectStrategy is not implemented yet.");

        if (action.TargetStrategy is not MouseTargetSelectStrategy mouseStrategy)
            return true;

        var result = await WaitForMouseTargets(mouseStrategy, confirmationToken);

        if (IsEndTurnRequested || selectedAction != action || result == null) return false;

        action.SetSelectedTargets(result);
        return true;
    }

    async UniTask<List<ActionTarget>?> WaitForMouseTargets(
        MouseTargetSelectStrategy mouseStrategy,
        CancellationToken confirmationToken)
    {
        using var targetSelectionCts = CancellationTokenSource.CreateLinkedTokenSource(confirmationToken);
        System.Action cancelTargetSelection = targetSelectionCts.Cancel;

        cancelCurrentTargetSelection = cancelTargetSelection;

        try
        {
            return await mouseStrategy.GetTarget(targetSelectionCts.Token);
        }
        finally
        {
            if (cancelCurrentTargetSelection == cancelTargetSelection)
                cancelCurrentTargetSelection = null;
        }
    }

    #endregion

    #region Action Selection

    void SelectAction(CharacterAction action)
    {
        if (!isAwaitingActionConfirmation) return;
        if (!CharacterManager.CanAffordMana(ActiveCharacterRunTimeId, action.ManaCost)) return;

        var previousTargetSelection = cancelCurrentTargetSelection;
        selectedAction = null;
        previousTargetSelection?.Invoke();

        action.Reset();
        selectedAction = action;
        UpdateActionButtonSelection();
        UpdateActionRangeIndicator();
        actionSelectionSource?.TrySetResult();
    }

    protected override void OnActionExecuted()
    {
        selectedAction = null;
        UpdateActionButtonSelection();
        UpdateActionRangeIndicator();
        UpdateManaDisplay(ActiveCharacterRunTimeId);
        UpdateActionButtonAffordability();
    }

    #endregion

    #region UI

    void UpdateActionButtonSelection()
    {
        ActiveGameView.UpdateActionButtonSelection(selectedAction);
    }

    void UpdateManaDisplay(int runTimeId)
    {
        ActiveGameView.SetManaText(
            CharacterManager.GetCurrentMana(runTimeId),
            CharacterManager.GetMaxMana(runTimeId));
    }

    void UpdateActionButtonAffordability()
    {
        ActiveGameView.UpdateActionButtonAffordability(CharacterManager.GetCurrentMana(ActiveCharacterRunTimeId));
    }

    void HandleActionHover(CharacterAction action)
    {
        if (!CharacterManager.CanAffordMana(ActiveCharacterRunTimeId, action.ManaCost)) return;

        action.TargetStrategy.ShowTargetPreview();
    }

    void HandleActionHoverExit()
    {
        if (selectedAction != null)
        {
            UpdateActionRangeIndicator();
            return;
        }

        HideActionRangeIndicator();
    }

    void UpdateActionRangeIndicator()
    {
        if (selectedAction == null)
        {
            HideActionRangeIndicator();
            return;
        }

        if (!CharacterManager.TryGetCharacter(ActiveCharacterRunTimeId, out var character))
        {
            HideActionRangeIndicator();
            return;
        }

        TargetSelectDisplay.SetActionRangeIndicator(character.Position, selectedAction.TargetStrategy);
    }

    protected override void HideActionRangeIndicator()
    {
        TargetSelectDisplay.SetActionRangeIndicatorVisible(false);
    }

    void HandleEndTurnButtonPressed()
    {
        RequestEndTurn();
        actionConfirmationCts?.Cancel();
        cancelCurrentTargetSelection?.Invoke();
        actionSelectionSource?.TrySetResult();
    }

    #endregion
}
