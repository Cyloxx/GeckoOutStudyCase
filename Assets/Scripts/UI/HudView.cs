using GeckoOut.Core.Session;
using TMPro;
using UnityEngine;

namespace GeckoOut.UI
{
    /// <summary>Top bar: level number and countdown. Polls the session timer.</summary>
    public class HudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _timerText;

        private LevelSession _session;

        public void Bind(LevelSession session, int levelNumber)
        {
            _session = session;
            _levelText.text = "Level " + levelNumber;
        }

        private void Update()
        {
            if (_session == null)
            {
                return;
            }

            int totalSeconds = Mathf.CeilToInt(_session.RemainingSeconds);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            _timerText.text = minutes + ":" + seconds.ToString("00");
        }
    }
}