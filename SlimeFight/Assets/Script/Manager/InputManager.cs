#nullable enable
using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public event Action<Vector2>? OnMouseClick;
    public event Action<Vector2>? OnDragStart;
    public event Action<Vector2>? OnDrag;
    public event Action? OnDragEnd;
    public event Action<float>? OnMouseScroll;

    private Camera _cam = null!;

    private const float ClickDragThresholdPixels = 10f;

    private Vector2 _mouseDownScreenPosition;
    private bool _isMouseDown;
    private bool _isDragging;

    public void Initialize()
    {
        _cam = Camera.main!;
    }

    private void Update()
    {
        HandleMouseDown();
        HandleMouseHeld();
        HandleMouseUp();
        HandleMouseScroll();
    }

    private void HandleMouseDown()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        _mouseDownScreenPosition = Input.mousePosition;
        _isMouseDown = true;
        _isDragging = false;
    }

    private void HandleMouseHeld()
    {
        if (!_isMouseDown || !Input.GetMouseButton(0))
            return;

        Vector2 currentMousePosition = Input.mousePosition;

        if (!_isDragging && HasExceededDragThreshold(currentMousePosition))
        {
            _isDragging = true;
            OnDragStart?.Invoke(currentMousePosition);
        }

        if (_isDragging)
            OnDrag?.Invoke(currentMousePosition);
    }

    private void HandleMouseScroll()
    {
        float scrollDelta = Input.mouseScrollDelta.y;
        if (scrollDelta != 0f)
            OnMouseScroll?.Invoke(scrollDelta);
    }

    private void HandleMouseUp()
    {
        if (!_isMouseDown || !Input.GetMouseButtonUp(0))
            return;

        Vector2 releasedMousePosition = Input.mousePosition;

        _isMouseDown = false;

        if (_isDragging)
        {
            _isDragging = false;
            OnDragEnd?.Invoke();
            return;
        }

        OnMouseClick?.Invoke(ScreenToWorldPoint(releasedMousePosition));
    }

    private bool HasExceededDragThreshold(Vector2 currentMousePosition)
    {
        return Vector2.Distance(_mouseDownScreenPosition, currentMousePosition) > ClickDragThresholdPixels;
    }

    private Vector2 ScreenToWorldPoint(Vector2 screenPosition)
    {
        Vector3 screenPoint = new Vector3(screenPosition.x, screenPosition.y, GetScreenToWorldZDistance());
        return _cam.ScreenToWorldPoint(screenPoint);
    }

    private float GetScreenToWorldZDistance()
    {
        return Mathf.Abs(_cam.transform.position.z);
    }
}