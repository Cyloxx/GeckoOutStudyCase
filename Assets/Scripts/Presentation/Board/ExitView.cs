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
    }
}