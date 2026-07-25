using System;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Core.Session;
using GeckoOut.Presentation.Board;
using GeckoOut.Presentation.Gecko;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GeckoOut.Presentation.Input
{
    /// <summary>
    /// Translates pointer input into session intents. The model still commits
    /// discrete steps toward the finger's nearest cell; this class additionally
    /// feeds the view a sub-cell "lead" (which neighbour, how far) so the
    /// grabbed gecko can follow the finger continuously between cells.
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
        private GeckoViewManager _viewManager;

        private GeckoBody _draggedGecko;
        private GeckoEnd _draggedEnd;
        private float _lastBlockedFeedbackTime;
        private float _grabRadiusInCells = 0.75f;

        public void Initialize(LevelSession session, BoardRaycaster raycaster,
                               BoardLayout layout, GeckoViewManager viewManager)
        {
            _session = session;
            _raycaster = raycaster;
            _layout = layout;
            _viewManager = viewManager;
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
                if (_draggedGecko != null)
                {
                    if (GeckoReleased != null)
                    {
                        GeckoReleased();
                    }

                    _viewManager.ClearDragRender(_draggedGecko);
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
            if (!_raycaster.TryGetWorldPoint(screenPosition, out Vector3 fingerWorld))
            {
                return;
            }

            GridPosition fingerCell = _layout.WorldToCell(fingerWorld);
            GridPosition headCell = _draggedGecko.GetEnd(_draggedEnd);

            if (!fingerCell.Equals(headCell))
            {
                bool moved = _session.TryDragTo(_draggedGecko, _draggedEnd, fingerCell);

                if (!moved)
                {
                    RaiseBlockedFeedback();
                }
            }

            UpdateDragRender(fingerWorld);
        }

        private void UpdateDragRender(Vector3 fingerWorld)
        {
            GridPosition headCell = _draggedGecko.GetEnd(_draggedEnd);
            Vector3 offset = fingerWorld - _layout.CellToWorld(headCell);

            GridPosition dir;
            float proj;

            if (Mathf.Abs(offset.x) >= Mathf.Abs(offset.z))
            {
                dir = new GridPosition(offset.x >= 0f ? 1 : -1, 0);
                proj = Mathf.Abs(offset.x) / _layout.CellSize;
            }
            else
            {
                dir = new GridPosition(0, offset.z >= 0f ? 1 : -1);
                proj = Mathf.Abs(offset.z) / _layout.CellSize;
            }

            GridPosition candidate = headCell.Add(dir);

            bool hasLead = IsNeckCell(candidate)
                || _session.CanStep(_draggedGecko, _draggedEnd, candidate);

            float progress = hasLead ? Mathf.Clamp(proj, 0f, 0.5f) : 0f;

            _viewManager.SetDragRender(_draggedGecko, _draggedEnd, candidate, hasLead, progress);
        }

        private bool IsNeckCell(GridPosition cell)
        {
            var cells = _draggedGecko.Cells;

            if (cells.Count < 2)
            {
                return false;
            }

            GridPosition neck = _draggedEnd == GeckoEnd.Head
                ? cells[1]
                : cells[cells.Count - 2];

            return cell.Equals(neck);
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