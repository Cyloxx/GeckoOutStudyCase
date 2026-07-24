using System;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Core.Session;
using GeckoOut.Presentation.Board;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GeckoOut.Presentation.Input
{
    /// <summary>
    /// Translates pointer input (mouse or touch) into session intents.
    /// Contains zero game rules: it only figures out which gecko end was
    /// grabbed and tells the session where that end wants to go.
    /// </summary>
    public class DragInputController : MonoBehaviour
    {
        private LevelSession _session;
        private BoardRaycaster _raycaster;

        private GeckoBody _draggedGecko;
        private GeckoEnd _draggedEnd;
        
        private BoardLayout _layout;
        private float _grabRadiusInCells = 0.75f;
        
        public event Action<GeckoBody, GeckoEnd> GeckoGrabbed;
        public event Action GeckoReleased;

        public void Initialize(LevelSession session, BoardRaycaster raycaster, BoardLayout layout)
        {
            _session = session;
            _raycaster = raycaster;
            _layout = layout;
            _draggedGecko = null;
        }

        private void Update()
        {
            if (_session == null)
            {
                return;
            }

            Pointer pointer = Pointer.current;

            if (pointer == null)
            {
                return;
            }

            Vector2 screenPosition = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                TryBeginDrag(screenPosition);
                return;
            }

            if (pointer.press.isPressed && _draggedGecko != null)
            {
                ContinueDrag(screenPosition);
                return;
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                if (_draggedGecko != null && GeckoReleased != null)
                {
                    GeckoReleased();
                }

                _draggedGecko = null;
            }
        }

        private void TryBeginDrag(Vector2 screenPosition)
        {
            if (!_raycaster.TryGetWorldPoint(screenPosition, out Vector3 worldPoint))
            {
                return;
            }

            float bestDistanceSqr = float.MaxValue;
            GeckoBody bestGecko = null;
            GeckoEnd bestEnd = GeckoEnd.Head;

            foreach (GeckoBody gecko in _session.ActiveGeckos)
            {
                ConsiderEnd(gecko, GeckoEnd.Head, worldPoint, ref bestDistanceSqr, ref bestGecko, ref bestEnd);
                ConsiderEnd(gecko, GeckoEnd.Tail, worldPoint, ref bestDistanceSqr, ref bestGecko, ref bestEnd);
            }

            float grabRadiusWorld = _grabRadiusInCells * _layout.CellSize;

            if (bestGecko != null && bestDistanceSqr <= grabRadiusWorld * grabRadiusWorld)
            {
                _draggedGecko = bestGecko;
                _draggedEnd = bestEnd;

                if (GeckoGrabbed != null)
                {
                    GeckoGrabbed(_draggedGecko, _draggedEnd);
                }
            }
            
        }

        private void ConsiderEnd(GeckoBody gecko, GeckoEnd end, Vector3 worldPoint,
            ref float bestDistanceSqr, ref GeckoBody bestGecko, ref GeckoEnd bestEnd)
        {
            Vector3 endWorld = _layout.CellToWorld(gecko.GetEnd(end));
            float distanceSqr = (endWorld - worldPoint).sqrMagnitude;

            if (distanceSqr < bestDistanceSqr)
            {
                bestDistanceSqr = distanceSqr;
                bestGecko = gecko;
                bestEnd = end;
            }
        }

        private void ContinueDrag(Vector2 screenPosition)
        {
            if (!_raycaster.TryGetCellUnderScreenPoint(screenPosition, out GridPosition cell))
            {
                return;
            }

            if (cell.Equals(_draggedGecko.GetEnd(_draggedEnd)))
            {
                return;
            }

            _session.TryDragTo(_draggedGecko, _draggedEnd, cell);
        }
    }
}