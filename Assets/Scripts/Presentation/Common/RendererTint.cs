using UnityEngine;

namespace GeckoOut.Presentation.Common
{
    /// <summary>
    /// Tints a renderer through a MaterialPropertyBlock, so materials are
    /// never duplicated per instance. Single home of this technique.
    /// </summary>
    public static class RendererTint
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        public static void SetBaseColor(Renderer renderer, Color color)
        {
            var propertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorProperty, color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}