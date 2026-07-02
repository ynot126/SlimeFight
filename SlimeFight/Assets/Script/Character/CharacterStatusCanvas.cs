using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterStatusCanvas : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI characterHealthText = null!;
    [SerializeField] CanvasGroup canvasGroup = null!;
    [SerializeField] float worldAnchorHeight = 0.5f;
    [SerializeField] Vector2 screenOffset = new(0f, 40f);
    [SerializeField] Vector2 screenPadding = new(16f, 16f);

    RectTransform rectTransform = null!;
    RectTransform canvasRect = null!;

    public void Initialize(RectTransform aCanvasRect)
    {
        canvasRect = aCanvasRect;
        rectTransform = (RectTransform)transform;
        SetVisible(false);
    }

    public void SetVisible(bool val)
    {
        canvasGroup.alpha = val ? 1 : 0;
    }

    public void UpdateStatus(Character character)
    {
        characterHealthText.text = $"Health: {character.CurrentHealth}/{character.MaxHealth}";
    }

    public void AnchorToWorldPosition(Vector3 worldPosition, Camera camera)
    {
        if (canvasRect == null || camera == null) return;

        var anchorWorld = worldPosition + Vector3.up * worldAnchorHeight;
        var screenPoint = camera.WorldToScreenPoint(anchorWorld);
        if (screenPoint.z < 0f)
        {
            SetVisible(false);
            return;
        }

        PlaceAtScreenPosition(screenPoint);
    }

    void PlaceAtScreenPosition(Vector2 screenPoint)
    {
        PlaceTooltip(canvasRect, rectTransform, screenPoint, screenOffset, screenPadding);
    }

    static bool PlaceTooltip(
        RectTransform canvasRect,
        RectTransform tooltip,
        Vector2 screenPoint,
        Vector2 offset,
        Vector2 padding)
    {
        if (canvasRect == null || tooltip == null)
            return false;

        screenPoint += offset;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                null,
                out Vector2 localPoint))
        {
            return false;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltip);

        Vector2 size = tooltip.rect.size;
        Vector2 pivot = tooltip.pivot;
        Rect canvas = canvasRect.rect;

        Vector2 anchored = localPoint;

        var minX = canvas.xMin + padding.x + size.x * pivot.x;
        var maxX = canvas.xMax - padding.x - size.x * (1f - pivot.x);
        var minY = canvas.yMin + padding.y + size.y * pivot.y;
        var maxY = canvas.yMax - padding.y - size.y * (1f - pivot.y);

        anchored.x = Mathf.Clamp(anchored.x, minX, maxX);
        anchored.y = Mathf.Clamp(anchored.y, minY, maxY);

        tooltip.anchoredPosition = anchored;
        return true;
    }
}
