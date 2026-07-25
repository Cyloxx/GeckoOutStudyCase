using UnityEngine;

namespace GeckoOut.Presentation.Audio
{
    /// <summary>
    /// Plays the game's one-shot sounds. No singleton and no global access:
    /// the composition root owns it and routes events into it, so audio
    /// stays a leaf of the dependency graph.
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;

        [Header("Clips")]
        [SerializeField] private AudioClip _grabClip;
        [SerializeField] private AudioClip _stepClip;
        [SerializeField] private AudioClip _blockedClip;
        [SerializeField] private AudioClip _exitClip;
        [SerializeField] private AudioClip _winClip;
        [SerializeField] private AudioClip _loseClip;
        [SerializeField] private AudioClip _timerTickClip;

        [Header("Tuning")]
        [SerializeField] private float _stepMinInterval = 0.05f;
        [SerializeField] private float _stepVolume = 0.5f;
        [SerializeField] private Vector2 _stepPitchRange = new Vector2(0.92f, 1.08f);

        private float _lastStepTime;

        public void PlayGrab()
        {
            Play(_grabClip, 1f);
        }

        public void PlayStep()
        {
            if (Time.time - _lastStepTime < _stepMinInterval)
            {
                return;
            }

            _lastStepTime = Time.time;

            _source.pitch = Random.Range(_stepPitchRange.x, _stepPitchRange.y);
            Play(_stepClip, _stepVolume);
            _source.pitch = 1f;
        }

        public void PlayBlocked()
        {
            Play(_blockedClip, 1f);
        }

        public void PlayExit()
        {
            Play(_exitClip, 1f);
        }

        public void PlayWin()
        {
            Play(_winClip, 1f);
        }

        public void PlayLose()
        {
            Play(_loseClip, 1f);
        }

        public void PlayTimerTick()
        {
            Play(_timerTickClip, 0.7f);
        }

        private void Play(AudioClip clip, float volume)
        {
            if (_source == null || clip == null)
            {
                return;
            }

            _source.PlayOneShot(clip, volume);
        }
    }
}