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
      var overUI = IsPointerOverUI();

      if (Input.GetMouseButtonUp(0) && isDragging)
      {
         isDragging = false;
         OnDragEnd?.Invoke();
      }

      if (overUI)
      {
         if (isDragging)
         {
            isDragging = false;
            OnDragEnd?.Invoke();
         }
         return;
      }

      if (!TryGetGroundMousePosition(out var worldMousePosition)) return;

      currentMousePosition = worldMousePosition;
      OnMousePositionUpdate?.Invoke(worldMousePosition);

      if (Input.GetMouseButtonDown(0))
         OnMouseClick?.Invoke(worldMousePosition);

      if (Input.GetMouseButtonDown(0))
      {
         isDragging = true;
         OnDragStart?.Invoke(worldMousePosition);
      }
      if (isDragging && Input.GetMouseButton(0))
         OnDragUpdate?.Invoke(worldMousePosition);
   }

   bool IsPointerOverUI()
   {
      var eventSystem = EventSystem.current;
      if (eventSystem == null) return false;
      return eventSystem.IsPointerOverGameObject(-1);
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
