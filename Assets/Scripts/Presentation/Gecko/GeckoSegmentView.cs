using DG.Tweening;
using GeckoOut.Presentation.Common;
using UnityEngine;

namespace GeckoOut.Presentation.Gecko
{
    /// <summary>
    /// One piece of a gecko: a plain ball for the body, or the head prefab.
    /// The root transform is reserved for tweens (grab pop, bumps); the visual
    /// child carries motion stretch and facing, so the two never fight over the
    /// same property. Size comes from the prefab, never from code.
    /// </summary>
    public class GeckoSegmentView : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Transform _visual;
        [SerializeField] private float _turnSpeed = 16f;

        [SerializeField] private Transform _connector;

        private Vector3 _connectorRestingScale = Vector3.one;
        private float _connectorRestingHeight;
        
        private Vector3 _restingScale = Vector3.one;

        /// <summary>The scale authored on the prefab; tweens return to it.</summary>
        public Vector3 RestingScale
        {
            get { return _restingScale; }
        }

        private void Awake()
        {
            _restingScale = transform.localScale;

            if (_connector != null)
            {
                _connectorRestingScale = _connector.localScale;
                _connectorRestingHeight = _connector.localPosition.y;
            }
        }

        public void SetColor(Color color)
        {
            if (_renderers == null)
            {
                return;
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    RendererTint.SetBaseColor(_renderers[i], color);
                }
            }
        }

        /// <summary>
        /// Turns the visual towards the given direction and squashes it along
        /// that axis. The turn is eased, so corners do not snap; the facing is
        /// kept while standing still.
        /// </summary>
        /// <summary>
        /// Turns the visual towards the given direction. The turn is eased, so
        /// corners do not snap, and the facing is kept while standing still.
        /// </summary>
        public void SetFacing(Vector3 direction, float deltaSeconds)
        {
            if (_visual == null || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
            float t = 1f - Mathf.Exp(-_turnSpeed * Mathf.Max(deltaSeconds, 0f));

            _visual.localRotation = Quaternion.Slerp(_visual.localRotation, target, t);
        }

        /// <summary>Faces a direction immediately, without easing (used on spawn).</summary>
        public void SnapFacing(Vector3 direction)
        {
            if (_visual == null || direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            _visual.localRotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        /// <summary>Restores the prefab's authored look before pooling.</summary>
        public void ResetVisual()
        {
            transform.DOKill();
            transform.localScale = _restingScale;

            if (_visual != null)
            {
                _visual.localRotation = Quaternion.identity;
                _visual.localScale = Vector3.one;
            }
        }
        
        /// <summary>
        /// Stretches this piece's neck so it reaches the next piece, which is
        /// what makes a coiled gecko readable as one connected body.
        /// </summary>
        public void SetConnectorTarget(Vector3 worldTarget)
        {
            if (_connector == null)
            {
                return;
            }

            Vector3 delta = worldTarget - transform.position;
            float distance = delta.magnitude;

            if (distance <= 0.0001f)
            {
                _connector.gameObject.SetActive(false);
                return;
            }

            float lossy = Mathf.Max(transform.lossyScale.z, 0.0001f);
            Vector3 localHalf = delta * 0.5f / lossy;

            _connector.gameObject.SetActive(true);
            _connector.localPosition = new Vector3(localHalf.x, _connectorRestingHeight, localHalf.z);
            _connector.rotation = Quaternion.LookRotation(delta, Vector3.up);
            _connector.localScale = new Vector3(
                _connectorRestingScale.x,
                _connectorRestingScale.y,
                distance / lossy);
        }

        public void HideConnector()
        {
            if (_connector != null)
            {
                _connector.gameObject.SetActive(false);
            }
        }
    }
}