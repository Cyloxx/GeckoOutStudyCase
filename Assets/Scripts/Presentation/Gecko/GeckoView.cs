using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Presentation.Board;
using GeckoOut.Presentation.Common;
using UnityEngine;

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

        private readonly GeckoBody _body;
        private readonly BoardLayout _layout;
        private readonly ObjectPool<GeckoSegmentView> _segmentPool;
        private readonly List<GeckoSegmentView> _segments = new List<GeckoSegmentView>();
        private readonly float _moveSpeed;
        private readonly Queue<List<GridPosition>> _stepSnapshots
            = new Queue<List<GridPosition>>();

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

            Color baseColor = ColorPalette.ToUnityColor(body.Color);
            Color bodyColor = baseColor * 0.75f;

            for (int i = 0; i < body.Cells.Count; i++)
            {
                GeckoSegmentView segment = _segmentPool.Get();

                bool isHead = i == 0;
                segment.transform.position = _layout.CellToWorld(body.Cells[i]);
                segment.transform.localScale = Vector3.one * (isHead ? HeadScale : BodyScale);
                segment.SetColor(isHead ? baseColor : bodyColor);

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
        /// Moves every segment toward its target cell. Targets come from the
        /// oldest pending step snapshot, so segments visit each cell of the
        /// path in order; once all segments arrive, the next snapshot is taken.
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

            bool allSegmentsArrived = true;

            for (int i = 0; i < _segments.Count; i++)
            {
                Vector3 target = _layout.CellToWorld(targetCells[i]);
                Transform segmentTransform = _segments[i].transform;

                segmentTransform.position = Vector3.MoveTowards(
                    segmentTransform.position, target, _moveSpeed * deltaSeconds);

                if ((segmentTransform.position - target).sqrMagnitude > 0.0001f)
                {
                    allSegmentsArrived = false;
                }
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
    }
}