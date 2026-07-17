using System.Collections.Generic;
using GeckoOut.Core.Rules;
using GeckoOut.Core.Session;
using GeckoOut.Data;
using GeckoOut.Presentation.Board;
using GeckoOut.Presentation.Cameras;
using GeckoOut.Presentation.Gecko;
using UnityEngine;

namespace GeckoOut.App
{
    /// <summary>
    /// Composition root: the only place that news up and wires together
    /// the pieces of the game. Loads a level, builds its view and ticks
    /// the session. UI and input wiring will join in later phases.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private LevelCatalogSO _levelCatalog;
        [SerializeField] private BoardViewBuilder _boardViewBuilder;
        [SerializeField] private BoardCameraFitter _cameraFitter;
        [SerializeField] private int _startLevelIndex = 0;
        [SerializeField] private GeckoViewManager _geckoViewManager;

        private LevelSession _session;

        private void Awake()
        {
            if (_levelCatalog == null || _boardViewBuilder == null || _cameraFitter == null || _geckoViewManager == null)
            {
                Debug.LogError("[GameBootstrapper] Scene references are not set up.", this);
                enabled = false;
                return;
            }

        }

        private void Start()
        {
            LoadLevel(_startLevelIndex);
        }

        private void Update()
        {
            if (_session != null)
            {
                _session.Tick(Time.deltaTime);
            }
        }

        private void LoadLevel(int levelIndex)
        {
            TextAsset levelFile = _levelCatalog.GetLevelFile(levelIndex);

            if (levelFile == null)
            {
                Debug.LogError("[GameBootstrapper] No level file at index " + levelIndex, this);
                return;
            }

            LevelDefinition definition = new LevelDefinitionLoader().Load(levelFile.text);

            var validator = new LevelValidator();
            if (!validator.IsValid(definition, out List<string> errors))
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

            _boardViewBuilder.Build(level.Board);
            _cameraFitter.Fit(level.Board.Width, level.Board.Height,
                _boardViewBuilder.Layout.CellSize);
            
            _geckoViewManager.Initialize(_session, _boardViewBuilder.Layout);
        }
    }
}