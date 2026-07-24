using System;
using GeckoOut.Core.Board;
using GeckoOut.Presentation.Board;
using UnityEngine;

namespace GeckoOut.Presentation.Input
{
    /// <summary>
    /// Converts a screen point into a grid cell by intersecting the camera
    /// ray with the board plane (y = 0). Pure math — no colliders, no physics.
    /// </summary>
    public class BoardRaycaster
    {
        private readonly Camera _camera;
        private readonly BoardLayout _layout;

        public BoardRaycaster(Camera camera, BoardLayout layout)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            if (layout == null)
            {
                throw new ArgumentNullException(nameof(layout));
            }

            _camera = camera;
            _layout = layout;
        }

        public bool TryGetCellUnderScreenPoint(Vector2 screenPoint, out GridPosition cell)
        {
            cell = default;

            if (!TryGetWorldPoint(screenPoint, out Vector3 worldPoint))
            {
                return false;
            }

            cell = _layout.WorldToCell(worldPoint);
            return true;
        }
        
        public bool TryGetWorldPoint(Vector2 screenPoint, out Vector3 worldPoint)
        {
            worldPoint = default;

            Ray ray = _camera.ScreenPointToRay(screenPoint);
            var boardPlane = new Plane(Vector3.up, 0f);

            if (!boardPlane.Raycast(ray, out float distanceAlongRay))
            {
                return false;
            }

            worldPoint = ray.GetPoint(distanceAlongRay);
            return true;
        }
        
        
    }
}