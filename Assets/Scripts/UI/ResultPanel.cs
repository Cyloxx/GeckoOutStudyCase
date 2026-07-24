using System;
using DG.Tweening;
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
        [SerializeField] private RectTransform _content;
        [SerializeField] private float _popDuration = 0.35f;

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

            if (_content == null)
            {
                return;
            }

            _content.DOKill();
            _content.localScale = Vector3.one * 0.6f;
            _content.DOScale(1f, _popDuration).SetEase(Ease.OutBack);
        }

        public void Hide()
        {
            if (_content != null)
            {
                _content.DOKill();
            }

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