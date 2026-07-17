using System.Collections;
using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Core.Session;
using GeckoOut.Presentation.Board;
using GeckoOut.Presentation.Common;
using UnityEngine;

namespace GeckoOut.Presentation.Gecko
{
    /// <summary>
    /// Creates and drives all gecko views for the running session, and
    /// plays the sink-into-the-hole animation when a gecko exits.
    /// </summary>
    public class GeckoViewManager : MonoBehaviour
    {
        [SerializeField] private GeckoSegmentView _segmentPrefab;
        [SerializeField] private Transform _geckoRoot;
        [SerializeField] private float _segmentMoveSpeed = 14f;
        [SerializeField] private float _sinkDurationPerSegment = 0.09f;

        private ObjectPool<GeckoSegmentView> _segmentPool;
        private readonly List<GeckoView> _views = new List<GeckoView>();
        private LevelSession _session;
        private BoardLayout _layout;

        public void Initialize(LevelSession session, BoardLayout layout)
        {
            Clear();

            if (_segmentPool == null)
            {
                _segmentPool = new ObjectPool<GeckoSegmentView>(_segmentPrefab, _geckoRoot);
            }

            _session = session;
            _layout = layout;
            _session.GeckoExited += HandleGeckoExited;

            foreach (GeckoBody gecko in session.ActiveGeckos)
            {
                _views.Add(new GeckoView(gecko, layout, _segmentPool, _segmentMoveSpeed));
            }
        }

        private void Update()
        {
            for (int i = 0; i < _views.Count; i++)
            {
                _views[i].Tick(Time.deltaTime);
            }
        }

        private void OnDestroy()
        {
            if (_session != null)
            {
                _session.GeckoExited -= HandleGeckoExited;
            }
        }

        private void Clear()
        {
            if (_session != null)
            {
                _session.GeckoExited -= HandleGeckoExited;
                _session = null;
            }

            foreach (GeckoView view in _views)
            {
                view.ReleaseAll();
            }

            _views.Clear();
            StopAllCoroutines();
        }

        private void HandleGeckoExited(GeckoBody gecko, ExitPoint exit)
        {
            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i].Body == gecko)
                {
                    GeckoView view = _views[i];
                    _views.RemoveAt(i);
                    StartCoroutine(PlaySinkRoutine(view, exit));
                    return;
                }
            }
        }

        private IEnumerator PlaySinkRoutine(GeckoView view, ExitPoint exit)
        {
            IReadOnlyList<GeckoSegmentView> segments = view.Segments;
            bool headExited = view.Body.Head.Equals(exit.Position);
            Vector3 holePosition = _layout.CellToWorld(exit.Position);

            int count = segments.Count;

            for (int i = 0; i < count; i++)
            {
                GeckoSegmentView segment = headExited ? segments[i] : segments[count - 1 - i];

                Vector3 startPosition = segment.transform.position;
                Vector3 startScale = segment.transform.localScale;
                float elapsed = 0f;

                while (elapsed < _sinkDurationPerSegment)
                {
                    float t = elapsed / _sinkDurationPerSegment;

                    segment.transform.position = Vector3.Lerp(startPosition, holePosition, t);
                    segment.transform.localScale = startScale * (1f - t);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                segment.transform.localScale = startScale;
                _segmentPool.Release(segment);
            }

            view.ForgetSegments();
        }
    }
}