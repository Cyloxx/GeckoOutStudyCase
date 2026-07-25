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
    /// Translates pointer input into session intents: grab an end, then keep
    /// asking the session to bring that end to the cell under the finger.
    /// The session walks the path one legal step at a time and the view
    /// animates every step, so all movement uses the same single path.
    /// Contains no game rules.
    /// </summary>
    public class DragInputController : MonoBehaviour
    {
        private const float BlockedFeedbackCooldown = 0.35f;

        public event Action<GeckoBody, GeckoEnd> GeckoGrabbed;
        public event Action<GeckoBody, GeckoEnd> MoveBlocked;
        public event Action GeckoReleased;

        private LevelSession _session;
        private BoardRaycaster _raycaster;
        private BoardLayout _layout;

        private GeckoBody _draggedGecko;
        private GeckoEnd _draggedEnd;
        private float _lastBlockedFeedbackTime;
        private float _grabRadiusInCells = 0.75f;

        public void Initialize(LevelSession session, BoardRaycaster raycaster,
                               BoardLayout layout)
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
                ConsiderEnd(gecko, GeckoEnd.Head, worldPoint,
                    ref bestDistanceSqr, ref bestGecko, ref bestEnd);
                ConsiderEnd(gecko, GeckoEnd.Tail, worldPoint,
                    ref bestDistanceSqr, ref bestGecko, ref bestEnd);
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
            if (!_raycaster.TryGetCellUnderScreenPoint(screenPosition, out GridPosition fingerCell))
            {
                return;
            }

            GridPosition endCell = _draggedGecko.GetEnd(_draggedEnd);

            if (fingerCell.Equals(endCell))
            {
                return;
            }

            bool moved;

            if (_draggedGecko.Occupies(fingerCell))
            {
                // Finger pushed into the body: the gecko slides the other way.
                moved = _session.TryPushBack(_draggedGecko, _draggedEnd);
            }
            else
            {
                moved = _session.TryDragTo(_draggedGecko, _draggedEnd, fingerCell);
            }

            if (!moved)
            {
                RaiseBlockedFeedback();
            }
        }

        private void RaiseBlockedFeedback()
        {
            if (Time.time - _lastBlockedFeedbackTime < BlockedFeedbackCooldown)
            {
                return;
            }

            _lastBlockedFeedbackTime = Time.time;

            if (MoveBlocked != null)
            {
                MoveBlocked(_draggedGecko, _draggedEnd);
            }
        }
    }
}