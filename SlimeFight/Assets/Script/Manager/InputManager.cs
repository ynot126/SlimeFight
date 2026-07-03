#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
   public event Action<Vector3>? OnMousePositionUpdate;
   public event Action<Vector3>? OnMouseClick;
   public event Action<Vector3>? OnDragStart;
   public event Action<Vector3>? OnDragUpdate;
   public event Action? OnDragEnd;
   public Vector3 CurrentMousePosition => currentMousePosition;
   Vector3 currentMousePosition;
   Camera mainCamera = null!;
   bool isDragging;
   static readonly Plane GroundPlane = new(Vector3.up, Vector3.zero);

   public void Initialize(Camera aMainCamera)
   {
      mainCamera = aMainCamera;
   }

   public void Update()
   {
      if (Input.GetMouseButtonUp(1) && isDragging)
      {
         isDragging = false;
         OnDragEnd?.Invoke();
      }

      var overUI = EventSystem.current.IsPointerOverGameObject();
      if (!TryGetGroundMousePosition(out var worldMousePosition)) return;

      if (!overUI)
      {
         currentMousePosition = worldMousePosition;
         OnMousePositionUpdate?.Invoke(worldMousePosition);
         if (Input.GetMouseButtonDown(0))
            OnMouseClick?.Invoke(worldMousePosition);
      }

      if (Input.GetMouseButtonDown(0) && !overUI)
      {
         isDragging = true;
         OnDragStart?.Invoke(worldMousePosition);
      }
      if (isDragging && Input.GetMouseButton(0))
         OnDragUpdate?.Invoke(worldMousePosition);
   }

   bool TryGetGroundMousePosition(out Vector3 position)
   {
      var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
      if (!GroundPlane.Raycast(ray, out var distance))
      {
         position = default;
         return false;
      }
      position = ray.GetPoint(distance);
      return true;
   }
}
