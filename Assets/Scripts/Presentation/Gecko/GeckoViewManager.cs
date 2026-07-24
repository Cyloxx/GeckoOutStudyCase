using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        [SerializeField] private Cameras.CameraShake _cameraShake;

        private ObjectPool<GeckoSegmentView> _segmentPool;
        private readonly List<GeckoView> _views = new List<GeckoView>();
        private LevelSession _session;
        private BoardLayout _layout;
        
        private GeckoView _grabbedView;
        private GeckoEnd _grabbedEnd;

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
            _session.GeckoStepped += HandleGeckoStepped;

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
                _session.GeckoStepped -= HandleGeckoStepped;
            }
        }
        
        public void SetGrabbed(GeckoBody gecko, GeckoEnd end)
        {
            ClearGrabbed();

            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i].Body == gecko)
                {
                    _views[i].SetGrabbed(end);
                    _grabbedView = _views[i];
                    _grabbedEnd = end;
                    return;
                }
            }
        }

        public void ClearGrabbed()
        {
            if (_grabbedView != null)
            {
                _grabbedView.ClearGrab(_grabbedEnd);
                _grabbedView = null;
            }
        }

        private void Clear()
        {
            if (_session != null)
            {
                _session.GeckoExited -= HandleGeckoExited;
                _session.GeckoStepped -= HandleGeckoStepped;
                _session = null;
            }

            foreach (GeckoView view in _views)
            {
                view.ReleaseAll();
            }

            _views.Clear();
            _grabbedView = null;
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

                    if (_cameraShake)
                    {
                        _cameraShake.Shake();
                    }

                    StartCoroutine(PlaySinkRoutine(view, exit));
                    return;
                }
            }
        }
        
        private void HandleGeckoStepped(GeckoBody gecko)
        {
            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i].Body == gecko)
                {
                    _views[i].CaptureStepSnapshot();
                    return;
                }
            }
        }
        
        public void PlayBlocked(GeckoBody gecko, GeckoEnd end)
        {
            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i].Body == gecko)
                {
                    _views[i].PlayBlockedBump(end);
                    break;
                }
            }

            if (_cameraShake)
            {
                _cameraShake.Shake(0.05f, 0.12f);
            }
        }

        /// <summary>
        /// Slides the body into the hole along its own trail: every tick the
        /// whole chain advances one cell toward the exit and the front-most
        /// segment sinks. No diagonal shortcuts — segments follow the path.
        /// </summary>
        private IEnumerator PlaySinkRoutine(GeckoView view, ExitPoint exit)
        {
            bool headExited = view.Body.Head.Equals(exit.Position);

            var path = new List<GridPosition>(view.Body.Cells);
            var ordered = new List<GeckoSegmentView>(view.Segments);

            // Order both lists so index 0 is the end that entered the hole.
            if (!headExited)
            {
                path.Reverse();
                ordered.Reverse();
            }

            view.ForgetSegments();

            int count = ordered.Count;

            for (int tick = 1; tick <= count; tick++)
            {
                GeckoSegmentView sinking = ordered[tick - 1];

                sinking.transform.DOKill();
                sinking.transform.DOScale(0f, _sinkDurationPerSegment).SetEase(Ease.InQuad);

                for (int j = tick; j < count; j++)
                {
                    Vector3 target = _layout.CellToWorld(path[j - tick]);

                    ordered[j].transform.DOKill();
                    ordered[j].transform.DOMove(target, _sinkDurationPerSegment)
                        .SetEase(Ease.Linear);
                }

                yield return new WaitForSeconds(_sinkDurationPerSegment);

                sinking.transform.DOKill();
                sinking.transform.localScale = Vector3.one;
                _segmentPool.Release(sinking);
            }
        }
    }
}