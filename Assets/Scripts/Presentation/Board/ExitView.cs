using DG.Tweening;
using GeckoOut.Core.Board;
using GeckoOut.Presentation.Common;
using UnityEngine;

namespace GeckoOut.Presentation.Board
{
    /// <summary>Colors one exit hole to match its ColorId.</summary>
    public class ExitView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        public void Initialize(ColorId color)
        {
            RendererTint.SetBaseColor(_renderer, ColorPalette.ToUnityColor(color));
        }
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        /// <summary>Pulses once and then closes: a used hole is gone for good.</summary>
        public void PlayConsumed()
        {
            transform.DOKill();
            transform.localScale = _baseScale;

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(_baseScale * 1.35f, 0.12f).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
            sequence.OnComplete(Hide);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}