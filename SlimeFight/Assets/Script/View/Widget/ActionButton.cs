#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    const float pointerHoverDelay = 0.2f;

    [SerializeField] TextMeshProUGUI actionName = null!;
    [SerializeField] Image buttonImage = null!;
    CancellationTokenSource? pointerEnterDelayCts;
    bool pointerEnterTriggered;

    public event Action? OnPointerEnter;
    public event Action? OnPointerExit;
    public event Action? OnPointerClick;

    void OnDestroy()
    {
        CancelPointerEnterDelay();
    }

    public void Initialize(CharacterAction characterAction)
    {
        actionName.text = $"{characterAction.ActionName} ({characterAction.ManaCost})";
    }
    
    public void SetButtonSelectState(bool val)
    {
        actionName.color = val ? Color.red : Color.black;
    }

    public void SetButtonSelectable(bool val)
    {
        buttonImage.color = val? Color.white: Color.gray;
    }

    public void SetHoverState(bool val)
    {
        transform.DOScale(val?1.2f:1f, 0.2f);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        CancelPointerEnterDelay();
        pointerEnterTriggered = false;
        TriggerPointerEnterAfterDelay().Forget();
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        CancelPointerEnterDelay();
        if (!pointerEnterTriggered) return;

        pointerEnterTriggered = false;
        OnPointerExit?.Invoke();
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        OnPointerClick?.Invoke();
    }

    void CancelPointerEnterDelay()
    {
        pointerEnterDelayCts?.Cancel();
        pointerEnterDelayCts?.Dispose();
        pointerEnterDelayCts = null;
    }

    async UniTaskVoid TriggerPointerEnterAfterDelay()
    {
        pointerEnterDelayCts = new CancellationTokenSource();
        var token = pointerEnterDelayCts.Token;
        try
        {
            await UniTask.Delay((int)(pointerHoverDelay * 1000f), cancellationToken: token);
            pointerEnterTriggered = true;
            OnPointerEnter?.Invoke();
        }
        catch (OperationCanceledException)
        {
        }
    }
}