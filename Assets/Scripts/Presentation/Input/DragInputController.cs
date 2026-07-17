using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Core.Session;
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

        public void Initialize(LevelSession session, BoardRaycaster raycaster)
        {
            _session = session;
            _raycaster = raycaster;
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
                _draggedGecko = null;
            }
        }

        private void TryBeginDrag(Vector2 screenPosition)
        {
            if (!_raycaster.TryGetCellUnderScreenPoint(screenPosition, out GridPosition cell))
            {
                return;
            }

            foreach (GeckoBody gecko in _session.ActiveGeckos)
            {
                if (gecko.Head.Equals(cell))
                {
                    _draggedGecko = gecko;
                    _draggedEnd = GeckoEnd.Head;
                    return;
                }

                if (gecko.Tail.Equals(cell))
                {
                    _draggedGecko = gecko;
                    _draggedEnd = GeckoEnd.Tail;
                    return;
                }
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