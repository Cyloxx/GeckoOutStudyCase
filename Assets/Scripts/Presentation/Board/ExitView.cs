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

        public void PlayPulse()
        {
            transform.DOKill();
            transform.localScale = _baseScale;
            transform.DOScale(_baseScale * 1.35f, 0.12f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }
    }
}