#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class MouseTargetSelectStrategy : BaseTargetSelectStrategy
{
    UniTaskCompletionSource<List<ActionTarget>>? selectionTcs;

    public abstract ActionRangeType RangeType { get; }
    public abstract float Range { get; }

    public async UniTask<List<ActionTarget>?> GetTarget(CancellationToken ct)
    {
        selectionTcs = new UniTaskCompletionSource<List<ActionTarget>>();

        inputManager.OnMouseClick += HandleMouseClick;
        inputManager.OnMousePositionUpdate += HandleMousePositionUpdate;

        try
        {
            UpdateTargetDisplay(inputManager.CurrentMousePosition);

            using (ct.Register(CancelSelection))
                return await selectionTcs.Task;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        finally
        {
            inputManager.OnMouseClick -= HandleMouseClick;
            inputManager.OnMousePositionUpdate -= HandleMousePositionUpdate;
            selectionTcs = null;
            ClearTargetDisplay();
        }
    }

    void HandleMouseClick(Vector3 position)
    {
        if (!IsPositionValid(position)) return;

        var targetCharacterRunTimeId = -1;
        if (characterManager.TryGetCharacterAtPosition(position, characterRunTimeId, out var foundRunTimeId))
            targetCharacterRunTimeId = foundRunTimeId;

        selectionTcs?.TrySetResult(new List<ActionTarget> { new ActionTarget(position, targetCharacterRunTimeId) });
    }

    void HandleMousePositionUpdate(Vector3 mousePosition)
    {
        UpdateTargetDisplay(mousePosition);
    }

    void CancelSelection()
    {
        selectionTcs?.TrySetCanceled();
    }

    protected abstract void UpdateTargetDisplay(Vector3 mousePosition);
    protected abstract bool IsPositionValid(Vector3 position);

    protected virtual void ClearTargetDisplay()
    {
        characterActionDisplay.SetVisible(false);
    }
}
