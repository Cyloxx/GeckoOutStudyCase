using System.Collections.Generic;
using DG.Tweening;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Presentation.Board;
using GeckoOut.Presentation.Common;
using UnityEngine;

namespace GeckoOut.Presentation.Gecko
{
    /// <summary>
    /// Visual of one gecko: a chain of pooled pieces that follow the body's
    /// cells. Every model step is queued as a snapshot and animated in order,
    /// so the body always walks through each cell it passes through — there is
    /// one movement path, whether the step came from a short drag or a long one.
    /// </summary>
    public class GeckoView
    {
        private const float CatchUpPerPendingStep = 0.9f;
        private const float GrabPopDuration = 0.18f;
        private const float GrabReturnDuration = 0.12f;
        private const float GrabScaleMultiplier = 1.1f;

        private readonly GeckoBody _body;
        private readonly BoardLayout _layout;
        private readonly ObjectPool<GeckoSegmentView> _headPool;
        private readonly ObjectPool<GeckoSegmentView> _bodyPool;
        private readonly List<GeckoSegmentView> _segments = new List<GeckoSegmentView>();
        private readonly float _moveSpeed;
        private readonly Queue<List<GridPosition>> _stepSnapshots
            = new Queue<List<GridPosition>>();

        private GeckoSegmentView _headSegment;
        private Color _baseColor;

        public GeckoBody Body
        {
            get { return _body; }
        }

        public IReadOnlyList<GeckoSegmentView> Segments
        {
            get { return _segments; }
        }

        public GeckoView(GeckoBody body, BoardLayout layout,
            ObjectPool<GeckoSegmentView> headPool,
            ObjectPool<GeckoSegmentView> bodyPool, float moveSpeed)
        {
            _body = body;
            _layout = layout;
            _headPool = headPool;
            _bodyPool = bodyPool;
            _moveSpeed = moveSpeed;

            _baseColor = ColorPalette.ToUnityColor(body.Color);

            for (int i = 0; i < body.Cells.Count; i++)
            {
                bool isHead = i == 0;
                GeckoSegmentView segment = isHead ? _headPool.Get() : _bodyPool.Get();

                segment.ResetVisual();
                segment.transform.position = _layout.CellToWorld(body.Cells[i]);
                segment.SetColor(_baseColor);

                if (isHead)
                {
                    _headSegment = segment;
                }

                _segments.Add(segment);
            }

            SnapHeadFacing();
            UpdateConnectors();
        }

        /// <summary>
        /// Records where every cell of the body is right now. Called once per
        /// model step, so a multi-cell move becomes an ordered queue of
        /// waypoints instead of one jump to the destination.
        /// </summary>
        public void CaptureStepSnapshot()
        {
            _stepSnapshots.Enqueue(new List<GridPosition>(_body.Cells));
        }

        /// <summary>
        /// Eases every piece toward its target cell. Targets come from the
        /// oldest pending snapshot, so each cell of the path is visited in
        /// order; the smoothing rate scales with how many steps are still
        /// queued, so long moves catch up without skipping cells.
        /// </summary>
        public void Tick(float deltaSeconds)
        {
            IReadOnlyList<GridPosition> targetCells;

            if (_stepSnapshots.Count > 0)
            {
                targetCells = _stepSnapshots.Peek();
            }
            else
            {
                targetCells = _body.Cells;
            }

            float pendingSteps = _stepSnapshots.Count;
            float rate = _moveSpeed * (1f + pendingSteps * CatchUpPerPendingStep);
            float t = 1f - Mathf.Exp(-rate * deltaSeconds);

            bool allSegmentsArrived = true;

            for (int i = 0; i < _segments.Count; i++)
            {
                Vector3 target = _layout.CellToWorld(targetCells[i]);
                Transform segmentTransform = _segments[i].transform;

                Vector3 next = Vector3.Lerp(segmentTransform.position, target, t);

                if ((next - target).sqrMagnitude < 0.00025f)
                {
                    next = target;
                }
                else
                {
                    allSegmentsArrived = false;
                }

                segmentTransform.position = next;
            }

            UpdateHeadFacing(deltaSeconds);
            UpdateConnectors();

            if (allSegmentsArrived && _stepSnapshots.Count > 0)
            {
                _stepSnapshots.Dequeue();
            }
        }

        private void UpdateHeadFacing(float deltaSeconds)
        {
            if (_headSegment == null || _segments.Count < 2)
            {
                return;
            }

            // The head points away from the piece behind it, so it keeps
            // looking forward even while the gecko slides backwards.
            Vector3 facing = _segments[0].transform.position - _segments[1].transform.position;

            _headSegment.SetFacing(facing, deltaSeconds);
        }

        private void UpdateConnectors()
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                if (i + 1 < _segments.Count)
                {
                    _segments[i].SetConnectorTarget(_segments[i + 1].transform.position);
                }
                else
                {
                    _segments[i].HideConnector();
                }
            }
        }

        private void SnapHeadFacing()
        {
            if (_headSegment != null && TryGetHeadAxis(_body.Cells, out Vector3 headAxis))
            {
                _headSegment.SnapFacing(headAxis);
            }
        }

        /// <summary>The direction the head points away from the cell behind it.</summary>
        private bool TryGetHeadAxis(IReadOnlyList<GridPosition> cells, out Vector3 axis)
        {
            axis = Vector3.zero;

            if (cells == null || cells.Count < 2)
            {
                return false;
            }

            axis = _layout.CellToWorld(cells[0]) - _layout.CellToWorld(cells[1]);
            return true;
        }

        /// <summary>Returns all pieces to their pools (level teardown).</summary>
        public void ReleaseAll()
        {
            foreach (GeckoSegmentView segment in _segments)
            {
                ReleaseSegment(segment);
            }

            _segments.Clear();
        }

        /// <summary>
        /// Returns one piece to the pool it came from. The view is the only
        /// place that knows which piece is the head, so routing lives here.
        /// </summary>
        public void ReleaseSegment(GeckoSegmentView segment)
        {
            if (segment == null)
            {
                return;
            }

            segment.ResetVisual();

            if (segment == _headSegment)
            {
                _headPool.Release(segment);
            }
            else
            {
                _bodyPool.Release(segment);
            }
        }

        /// <summary>
        /// Forgets the pieces without releasing them. Used when the exit
        /// animation takes over their lifetime.
        /// </summary>
        public void ForgetSegments()
        {
            _segments.Clear();
        }

        public void SetGrabbed(GeckoEnd end)
        {
            int index = EndSegmentIndex(end);

            if (index < 0)
            {
                return;
            }

            GeckoSegmentView segment = _segments[index];

            segment.transform.DOKill();
            segment.transform.DOScale(segment.RestingScale * GrabScaleMultiplier, GrabPopDuration)
                .SetEase(Ease.OutBack);

            segment.SetColor(Color.Lerp(_baseColor, Color.white, 0.35f));
        }

        public void PlayBlockedBump(GeckoEnd end)
        {
            int index = EndSegmentIndex(end);

            if (index < 0)
            {
                return;
            }

            // The blocked end is always the grabbed one, so its resting size is
            // the grab size. Reset to it explicitly so bumps cannot accumulate.
            GeckoSegmentView segment = _segments[index];
            Vector3 grabbedScale = segment.RestingScale * GrabScaleMultiplier;

            segment.transform.DOKill();
            segment.transform.localScale = grabbedScale;
            segment.transform.DOScale(grabbedScale * 1.1f, 0.07f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }

        public void ClearGrab(GeckoEnd end)
        {
            int index = EndSegmentIndex(end);

            if (index < 0)
            {
                return;
            }

            GeckoSegmentView segment = _segments[index];

            segment.transform.DOKill();
            segment.transform.DOScale(segment.RestingScale, GrabReturnDuration)
                .SetEase(Ease.OutQuad);

            segment.SetColor(_baseColor);
        }

        private int EndSegmentIndex(GeckoEnd end)
        {
            if (_segments.Count == 0)
            {
                return -1;
            }

            return end == GeckoEnd.Head ? 0 : _segments.Count - 1;
        }
    }
}