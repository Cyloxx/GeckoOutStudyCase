using System;
using UnityEngine;
using UnityEngine.UI;

namespace GeckoOut.UI
{
    /// <summary>
    /// Full-screen end-of-level panel. Win and lose are two scene instances
    /// of this same script with different texts; the only behaviour is one
    /// action button that raises a C# event.
    /// </summary>
    public class ResultPanel : MonoBehaviour
    {
        [SerializeField] private Button _actionButton;

        public event Action ActionClicked;

        private void Awake()
        {
            _actionButton.onClick.AddListener(HandleActionButtonClicked);
        }

        private void OnDestroy()
        {
            _actionButton.onClick.RemoveListener(HandleActionButtonClicked);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void HandleActionButtonClicked()
        {
            if (ActionClicked != null)
            {
                ActionClicked();
            }
        }
    }
}