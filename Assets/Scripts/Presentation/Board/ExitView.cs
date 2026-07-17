using GeckoOut.Core.Board;
using GeckoOut.Presentation.Common;
using UnityEngine;

namespace GeckoOut.Presentation.Board
{
    /// <summary>Colors one exit hole to match its ColorId.</summary>
    public class ExitView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        public void Initialize(ColorId color)
        {
            var propertyBlock = new MaterialPropertyBlock();
            _renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorProperty, ColorPalette.ToUnityColor(color));
            _renderer.SetPropertyBlock(propertyBlock);
        }
    }
}