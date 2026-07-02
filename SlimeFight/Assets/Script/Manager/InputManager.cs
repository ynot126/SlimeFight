#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
   public event Action<Vector3>? OnMousePositionUpdate;
   public event Action<Vector3>? OnMouseClick;
   public Vector3 CurrentMousePosition => currentMousePosition;
   Vector3 currentMousePosition;
   Camera mainCamera = null!;
   static readonly Plane GroundPlane = new(Vector3.up, Vector3.zero);

   public void Initialize(Camera aMainCamera)
   {
      mainCamera = aMainCamera;
   }

   public void Update()
   {
      if (EventSystem.current.IsPointerOverGameObject()) return;
      if (!TryGetGroundMousePosition(out var worldMousePosition)) return;
      currentMousePosition = worldMousePosition;
      OnMousePositionUpdate?.Invoke(worldMousePosition);

      if (Input.GetMouseButtonDown(0))
         OnMouseClick?.Invoke(worldMousePosition);
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
