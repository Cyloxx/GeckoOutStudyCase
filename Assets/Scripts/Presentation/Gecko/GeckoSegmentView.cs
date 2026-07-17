using GeckoOut.Presentation.Common;
using UnityEngine;

namespace GeckoOut.Presentation.Gecko
{
    /// <summary>One ball of a gecko's body chain.</summary>
    public class GeckoSegmentView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        public void SetColor(Color color)
        {
            RendererTint.SetBaseColor(_renderer, color);
        }
    }
}