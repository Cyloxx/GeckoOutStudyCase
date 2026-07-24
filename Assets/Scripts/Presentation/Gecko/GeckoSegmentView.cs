using GeckoOut.Presentation.Common;
using UnityEngine;

namespace GeckoOut.Presentation.Gecko
{
    /// <summary>
    /// One ball of a gecko's body chain. The root transform is reserved for
    /// tweens (grab pop, bumps); the visual child carries motion stretch,
    /// so the two never fight over the same property.
    /// </summary>
    public class GeckoSegmentView : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;
        [SerializeField] private Transform _visual;

        public void SetColor(Color color)
        {
            RendererTint.SetBaseColor(_renderer, color);
        }

        /// <summary>
        /// Stretches the ball along its movement direction and squashes it
        /// sideways. Amount 0 restores the neutral shape.
        /// </summary>
        public void SetStretch(Vector3 direction, float amount)
        {
            if (_visual == null)
            {
                return;
            }

            if (amount <= 0.001f || direction.sqrMagnitude <= 0.0001f)
            {
                _visual.localRotation = Quaternion.identity;
                _visual.localScale = Vector3.one;
                return;
            }

            _visual.localRotation = Quaternion.LookRotation(direction, Vector3.up);
            _visual.localScale = new Vector3(
                1f - amount * 0.5f,
                1f - amount * 0.5f,
                1f + amount);
        }
    }
}