using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Presentation.Board;
using GeckoOut.Presentation.Common;
using UnityEngine;
using DG.Tweening;

namespace GeckoOut.Presentation.Gecko
{
    /// <summary>
    /// Visual of one gecko: a chain of pooled segments that follow the
    /// body's cells. Reads the body every frame and eases the segments
    /// toward their target cells, so model jumps become smooth motion.
    /// </summary>
    public class GeckoView
    {
        private const float HeadScale = 0.85f;
        private const float BodyScale = 0.68f;
        private const float CatchUpPerPendingStep = 0.9f;
        private const float GrabPopDuration = 0.18f;
        private const float GrabReturnDuration = 0.12f;

        private readonly GeckoBody _body;
        private readonly BoardLayout _layout;
        private readonly ObjectPool<GeckoSegmentView> _segmentPool;
        private readonly List<GeckoSegmentView> _segments = new List<GeckoSegmentView>();
        private readonly float _moveSpeed;
        private readonly Queue<List<GridPosition>> _stepSnapshots
            = new Queue<List<GridPosition>>();
        
        private const float GrabScaleMultiplier = 1.25f;

        private Color _baseColor;
        private Color _bodyColor;

        public GeckoBody Body
        {
            get { return _body; }
        }

        public IReadOnlyList<GeckoSegmentView> Segments
        {
            get { return _segments; }
        }

        public GeckoView(GeckoBody body, BoardLayout layout,
                         ObjectPool<GeckoSegmentView> segmentPool, float moveSpeed)
        {
            _body = body;
            _layout = layout;
            _segmentPool = segmentPool;
            _moveSpeed = moveSpeed;

            _baseColor = ColorPalette.ToUnityColor(body.Color);
            _bodyColor = _baseColor * 0.75f;

            for (int i = 0; i < body.Cells.Count; i++)
            {
                GeckoSegmentView segment = _segmentPool.Get();

                bool isHead = i == 0;
                segment.transform.position = _layout.CellToWorld(body.Cells[i]);
                segment.transform.localScale = Vector3.one * (isHead ? HeadScale : BodyScale);
                segment.SetColor(isHead ? _baseColor : _bodyColor);

                _segments.Add(segment);
            }
        }

        /// <summary>
        /// Records where every cell of the body is right now. Called once
        /// per model step, so multi-step drags become an ordered trail of
        /// waypoints instead of one big diagonal jump.
        /// </summary>
        public void CaptureStepSnapshot()
        {
            _stepSnapshots.Enqueue(new List<GridPosition>(_body.Cells));
        }
        
        /// <summary>
        /// Eases every segment toward its target cell. Uses exponential
        /// smoothing so motion starts quickly and decelerates into a soft
        /// stop; the rate scales with how many steps are still queued, so
        /// fast drags catch up to the finger while single steps stay gentle.
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

            if (allSegmentsArrived && _stepSnapshots.Count > 0)
            {
                _stepSnapshots.Dequeue();
            }
        }

        /// <summary>Returns all segments to the pool (level teardown).</summary>
        public void ReleaseAll()
        {
            foreach (GeckoSegmentView segment in _segments)
            {
                _segmentPool.Release(segment);
            }

            _segments.Clear();
        }

        /// <summary>
        /// Forgets the segments without releasing them. Used when the exit
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

            float baseScale = end == GeckoEnd.Head ? HeadScale : BodyScale;
            Transform segmentTransform = _segments[index].transform;

            segmentTransform.DOKill();
            segmentTransform.DOScale(baseScale * GrabScaleMultiplier, GrabPopDuration)
                .SetEase(Ease.OutBack);

            _segments[index].SetColor(Color.Lerp(EndBaseColor(end), Color.white, 0.35f));
        }

        public void ClearGrab(GeckoEnd end)
        {
            int index = EndSegmentIndex(end);
            if (index < 0)
            {
                return;
            }

            float baseScale = end == GeckoEnd.Head ? HeadScale : BodyScale;
            Transform segmentTransform = _segments[index].transform;

            segmentTransform.DOKill();
            segmentTransform.DOScale(baseScale, GrabReturnDuration)
                .SetEase(Ease.OutQuad);

            _segments[index].SetColor(EndBaseColor(end));
        }

        private int EndSegmentIndex(GeckoEnd end)
        {
            if (_segments.Count == 0)
            {
                return -1;
            }

            return end == GeckoEnd.Head ? 0 : _segments.Count - 1;
        }

        private Color EndBaseColor(GeckoEnd end)
        {
            return end == GeckoEnd.Head ? _baseColor : _bodyColor;
        }
    }
}