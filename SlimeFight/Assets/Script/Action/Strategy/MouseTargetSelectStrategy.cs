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
            UpdateTargetDisplayForMousePosition(inputManager.CurrentMousePosition);

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
        if (!mapManager.TryWorldToHex(position, out var hex)) return;
        if (!IsHexValid(hex)) return;

        var targetCharacterRunTimeId = -1;
        var snappedPosition = mapManager.HexToWorld(hex);
        if (characterManager.TryGetCharacterAtPosition(snappedPosition, characterRunTimeId, out var foundRunTimeId))
            targetCharacterRunTimeId = foundRunTimeId;

        selectionTcs?.TrySetResult(new List<ActionTarget> { new ActionTarget(hex, snappedPosition, targetCharacterRunTimeId) });
    }

    void HandleMousePositionUpdate(Vector3 mousePosition)
    {
        UpdateTargetDisplayForMousePosition(mousePosition);
    }

    void CancelSelection()
    {
        selectionTcs?.TrySetCanceled();
    }

    void UpdateTargetDisplayForMousePosition(Vector3 mousePosition)
    {
        if (!mapManager.TryWorldToHex(mousePosition, out var hex))
        {
            mapManager.ClearHover();
            return;
        }

        UpdateTargetDisplay(hex);
    }

    protected abstract void UpdateTargetDisplay(HexCoord hex);
    protected abstract bool IsHexValid(HexCoord hex);

    public override void ShowTargetPreview()
    {
        if (!characterManager.TryGetCharacterHex(characterRunTimeId, out var characterHex))
        {
            mapManager.ClearRange();
            return;
        }

        var hexes = mapManager.GetHexesInRange(characterHex, Mathf.RoundToInt(Range));
        mapManager.ShowRange(hexes);
    }

    protected virtual void ClearTargetDisplay()
    {
        mapManager.ClearRange();
        mapManager.ClearHover();
    }
}
