#nullable enable
using UnityEngine;

public class GameCameraController : MonoBehaviour
{
    InputManager inputManager = null!;
    bool draggingEnabled;
    bool isDragging;
    Vector3 dragAnchorWorldPosition;
    Bounds worldBounds;

    public void Initialize(InputManager aInputManager, Bounds mapBounds)
    {
        inputManager = aInputManager;
        worldBounds = mapBounds;
        transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        transform.position = ClampCameraPosition(transform.position);
        inputManager.OnDragStart += HandleDragStart;
        inputManager.OnDragUpdate += HandleDragUpdate;
        inputManager.OnDragEnd += HandleDragEnd;
    }

    void OnDestroy()
    {
        if (inputManager == null) return;
        inputManager.OnDragStart -= HandleDragStart;
        inputManager.OnDragUpdate -= HandleDragUpdate;
        inputManager.OnDragEnd -= HandleDragEnd;
    }

    public void SetDraggingEnabled(bool enabled)
    {
        draggingEnabled = enabled;
        if (!draggingEnabled)
            isDragging = false;
    }

    void HandleDragStart(Vector3 worldPosition)
    {
        if (!draggingEnabled) return;
        isDragging = true;
        dragAnchorWorldPosition = worldPosition;
    }

    void HandleDragUpdate(Vector3 worldPosition)
    {
        if (!draggingEnabled || !isDragging) return;
        // Keep the grabbed ground point under the cursor: translating the camera by
        // (anchor - currentHit) shifts the ground hit by the same amount.
        var delta = dragAnchorWorldPosition - worldPosition;
        transform.position = ClampCameraPosition(transform.position + delta);
    }

    void HandleDragEnd() => isDragging = false;

    Vector3 ClampCameraPosition(Vector3 desiredPosition)
    {
        var forward = transform.forward;
        if (Mathf.Abs(forward.y) < 0.0001f)
            return desiredPosition;

        var t = -desiredPosition.y / forward.y;
        if (t < 0f)
            return desiredPosition;

        var groundFocus = desiredPosition + forward * t;
        var clampedX = Mathf.Clamp(groundFocus.x, worldBounds.min.x, worldBounds.max.x);
        var clampedZ = Mathf.Clamp(groundFocus.z, worldBounds.min.z, worldBounds.max.z);
        var correction = new Vector3(clampedX - groundFocus.x, 0f, clampedZ - groundFocus.z);
        return desiredPosition + correction;
    }
}
