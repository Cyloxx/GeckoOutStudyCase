using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GeckoOut.Core.Board;
using GeckoOut.Core.Rules;
using GeckoOut.Core.Session;
using GeckoOut.Data;
using GeckoOut.Presentation.Board;
using GeckoOut.Presentation.Cameras;
using GeckoOut.Presentation.Gecko;
using GeckoOut.Presentation.Input;
using GeckoOut.UI;
using UnityEngine;
using GeckoOut.Core.Gecko;

namespace GeckoOut.App
{
    /// <summary>
    /// Composition root and level flow: the only place that news up and
    /// wires the pieces, loads levels in catalog order and reacts to
    /// win/lose by showing the result panels.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private LevelCatalogSO _levelCatalog;
        [SerializeField] private int _startLevelIndex = 0;

        [Header("Presentation")]
        [SerializeField] private BoardViewBuilder _boardViewBuilder;
        [SerializeField] private BoardCameraFitter _cameraFitter;
        [SerializeField] private GeckoViewManager _geckoViewManager;
        [SerializeField] private DragInputController _dragInputController;
        [SerializeField] private Camera _mainCamera;

        [Header("UI")]
        [SerializeField] private HudView _hudView;
        [SerializeField] private ResultPanel _winPanel;
        [SerializeField] private ResultPanel _losePanel;
        [SerializeField] private ParticleSystem _winParticlePrefab;
        [SerializeField] private float _winParticleHeight = 2f;

        [Header("Flow")]
        [SerializeField] private float _winPanelDelaySeconds = 1.2f;

        private LevelSession _session;
        private int _currentLevelIndex;
        
        

        private void Awake()
        {
            DOTween.Init();
            
            bool referencesMissing = _levelCatalog == null
                || _boardViewBuilder == null
                || _cameraFitter == null
                || _geckoViewManager == null
                || _dragInputController == null
                || _mainCamera == null
                || _hudView == null
                || _winPanel == null
                || _losePanel == null;

            if (referencesMissing)
            {
                Debug.LogError("[GameBootstrapper] Scene references are not set up.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            _winPanel.ActionClicked += HandleNextRequested;
            _losePanel.ActionClicked += HandleRetryRequested;

            _currentLevelIndex = _startLevelIndex;
            LoadLevel(_currentLevelIndex);
            _dragInputController.GeckoGrabbed += HandleGeckoGrabbed;
            _dragInputController.GeckoReleased += HandleGeckoReleased;
            
        }

        private void Update()
        {
            if (_session != null)
            {
                _session.Tick(Time.deltaTime);
            }
            _dragInputController.MoveBlocked += HandleMoveBlocked;
        }

        private void OnDestroy()
        {
            _winPanel.ActionClicked -= HandleNextRequested;
            _losePanel.ActionClicked -= HandleRetryRequested;

            UnsubscribeFromSession();
            
            _dragInputController.GeckoGrabbed -= HandleGeckoGrabbed;
            _dragInputController.GeckoReleased -= HandleGeckoReleased;
            _dragInputController.MoveBlocked -= HandleMoveBlocked;
        }
        
        private void HandleGeckoGrabbed(GeckoBody gecko, GeckoEnd end)
        {
            _geckoViewManager.SetGrabbed(gecko, end);
        }

        private void HandleGeckoReleased()
        {
            _geckoViewManager.ClearGrabbed();
        }
        private void HandleMoveBlocked(GeckoBody gecko, GeckoEnd end)
        {
            _geckoViewManager.PlayBlocked(gecko, end);
        }

        private void LoadLevel(int levelIndex)
        {
            UnsubscribeFromSession();

            _winPanel.Hide();
            _losePanel.Hide();

            TextAsset levelFile = _levelCatalog.GetLevelFile(levelIndex);

            if (levelFile == null)
            {
                Debug.LogError("[GameBootstrapper] No level file at index " + levelIndex, this);
                return;
            }

            LevelDefinition definition = new LevelDefinitionLoader().Load(levelFile.text);

            var levelValidator = new LevelValidator();
            if (!levelValidator.IsValid(definition, out List<string> errors))
            {
                foreach (string error in errors)
                {
                    Debug.LogError("[GameBootstrapper] Invalid level: " + error, this);
                }

                return;
            }

            LoadedLevel level = new LevelFactory().Create(definition);

            var moveValidator = new MoveValidator(level.Board, new ColorMatchExitRule());
            _session = new LevelSession(level.Board, level.Geckos,
                moveValidator, new PathResolver(), level.TimeLimitSeconds);

            _session.LevelWon += HandleLevelWon;
            _session.LevelLost += HandleLevelLost;
            _session.GeckoExited += HandleGeckoExited;

            _boardViewBuilder.Build(level.Board);
            _cameraFitter.Fit(level.Board.Width, level.Board.Height,
                _boardViewBuilder.Layout.CellSize);
            _geckoViewManager.Initialize(_session, _boardViewBuilder.Layout);

            var raycaster = new BoardRaycaster(_mainCamera, _boardViewBuilder.Layout);
            _dragInputController.Initialize(_session, raycaster, _boardViewBuilder.Layout);

            _hudView.Bind(_session, level.LevelId);
        }

        private void UnsubscribeFromSession()
        {
            if (_session == null)
            {
                return;
            }

            _session.LevelWon -= HandleLevelWon;
            _session.LevelLost -= HandleLevelLost;
            _session.GeckoExited -= HandleGeckoExited;
        }

        private void HandleLevelWon()
        {
            if (_winParticlePrefab)
            {
                ParticleSystem confetti = Instantiate(_winParticlePrefab,
                    new Vector3(0f, _winParticleHeight, 0f),
                    _winParticlePrefab.transform.rotation);
                confetti.Play();
                Destroy(confetti.gameObject, 4f);
            }

            StartCoroutine(ShowWinPanelAfterDelay());
        }
        
        private void HandleGeckoExited(GeckoBody gecko, ExitPoint exit)
        {
            _boardViewBuilder.PlayExitFeedback(exit.Position);
        }

        private IEnumerator ShowWinPanelAfterDelay()
        {
            // Let the last sink animation finish before covering the board.
            yield return new WaitForSeconds(_winPanelDelaySeconds);
            _winPanel.Show();
        }

        private void HandleLevelLost()
        {
            _losePanel.Show();
        }

        private void HandleNextRequested()
        {
            _currentLevelIndex = (_currentLevelIndex + 1) % _levelCatalog.LevelCount;
            LoadLevel(_currentLevelIndex);
        }

        private void HandleRetryRequested()
        {
            LoadLevel(_currentLevelIndex);
        }
    }
}