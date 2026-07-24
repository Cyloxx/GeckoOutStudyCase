using DG.Tweening;
using GeckoOut.Core.Session;
using TMPro;
using UnityEngine;

namespace GeckoOut.UI
{
    /// <summary>
    /// Top bar: level number and countdown. Polls the session timer and
    /// turns the clock into a pressure signal in the last seconds.
    /// </summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private float _urgentThresholdSeconds = 5f;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _urgentColor = new Color(0.95f, 0.25f, 0.2f);

        private LevelSession _session;
        private int _lastShownSecond = -1;
        private bool _isUrgent;

        public void Bind(LevelSession session, int levelNumber)
        {
            _session = session;
            _levelText.text = "Level " + levelNumber;

            _lastShownSecond = -1;
            SetUrgent(false);
        }

        private void Update()
        {
            if (_session == null)
            {
                return;
            }

            int totalSeconds = Mathf.CeilToInt(_session.RemainingSeconds);

            if (totalSeconds != _lastShownSecond)
            {
                _lastShownSecond = totalSeconds;
                RefreshTimerText(totalSeconds);
                HandleSecondTick(totalSeconds);
            }
        }

        private void RefreshTimerText(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            _timerText.text = minutes + ":" + seconds.ToString("00");
        }

        private void HandleSecondTick(int totalSeconds)
        {
            bool shouldBeUrgent = totalSeconds <= _urgentThresholdSeconds && totalSeconds > 0;

            if (shouldBeUrgent != _isUrgent)
            {
                SetUrgent(shouldBeUrgent);
            }

            if (shouldBeUrgent)
            {
                PulseTimer();
            }
        }

        private void SetUrgent(bool urgent)
        {
            _isUrgent = urgent;
            _timerText.color = urgent ? _urgentColor : _normalColor;

            if (!urgent)
            {
                _timerText.transform.DOKill();
                _timerText.transform.localScale = Vector3.one;
            }
        }

        private void PulseTimer()
        {
            Transform timerTransform = _timerText.transform;

            timerTransform.DOKill();
            timerTransform.localScale = Vector3.one;
            timerTransform.DOScale(1.25f, 0.12f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutQuad);
        }
    }
}