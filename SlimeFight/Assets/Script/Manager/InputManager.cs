#nullable enable
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
   public event Action<Vector2>? OnMousePositionUpdate;
   public event Action<Vector2>? OnMouseClick;
   public Vector2 CurrentMousePosition { get; private set; }
   Camera mainCamera = null!;
   public void Initialize(Camera aMainCamera)
   {
      mainCamera = aMainCamera;
   }

   public void Update()
   {
      if (EventSystem.current.IsPointerOverGameObject()) return;
      var worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
      CurrentMousePosition = worldMousePosition;
      OnMousePositionUpdate?.Invoke(worldMousePosition);

      if (Input.GetMouseButtonDown(0))
         OnMouseClick?.Invoke(worldMousePosition);
   }
}