using DG.Tweening;
using UnityEngine;

namespace GeckoOut.Presentation.Cameras
{
    /// <summary>
    /// Small reusable screen shake. Shakes a transform and returns it to
    /// its starting position. Used by exit, invalid-move and other feel moments.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private Transform _shakeTarget;
        [SerializeField] private float _defaultStrength = 0.25f;
        [SerializeField] private float _defaultDuration = 0.25f;

        public void Shake()
        {
            Shake(_defaultStrength, _defaultDuration);
        }

        public void Shake(float strength, float duration)
        {
            if (_shakeTarget == null)
            {
                return;
            }

            _shakeTarget.DOKill();
            _shakeTarget.DOShakePosition(duration, strength, 20, 90f, false, true);
        }
    }
}