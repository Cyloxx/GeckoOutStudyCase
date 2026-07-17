using System.Collections.Generic;
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

        /// <summary>Eases every segment toward its current cell. Called once per frame.</summary>
        public void Tick(float deltaSeconds)
        {
            for (int i = 0; i < _segments.Count; i++)
            {
                Vector3 target = _layout.CellToWorld(_body.Cells[i]);

                _segments[i].transform.position = Vector3.MoveTowards(
                    _segments[i].transform.position, target, _moveSpeed * deltaSeconds);
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