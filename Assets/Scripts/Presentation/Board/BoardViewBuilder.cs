using System.Collections.Generic;
using GeckoOut.Core.Board;
using UnityEngine;

namespace GeckoOut.Presentation.Board
{
    /// <summary>
    /// Spawns the static visuals of a level: floor tiles, walls and exit
    /// holes. Rebuilt from data on every level load — boards are data,
    /// not scenes.
    /// </summary>
    public class BoardViewBuilder : MonoBehaviour
    {
        [SerializeField] private GameObject _floorPrefab;
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private ExitView _exitPrefab;
        [SerializeField] private Transform _boardRoot;
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private ParticleSystem _exitParticlePrefab;

        private readonly Dictionary<GridPosition, ExitView> _exitViews
            = new Dictionary<GridPosition, ExitView>();

        public BoardLayout Layout { get; private set; }

        public void Build(BoardGrid board)
        {
            Clear();

            Layout = new BoardLayout(board.Width, board.Height, _cellSize);

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var cell = new GridPosition(x, y);
                    Vector3 position = Layout.CellToWorld(cell);

                    Instantiate(_floorPrefab, position, Quaternion.identity, _boardRoot);

                    if (board.IsWall(cell))
                    {
                        Instantiate(_wallPrefab, position, Quaternion.identity, _boardRoot);
                    }
                }
            }

            foreach (ExitPoint exit in board.Exits)
            {
                ExitView exitView = Instantiate(_exitPrefab,
                    Layout.CellToWorld(exit.Position), Quaternion.identity, _boardRoot);
                exitView.Initialize(exit.Color);
                _exitViews[exit.Position] = exitView;
            }
        }

        private void Clear()
        {
            _exitViews.Clear();
            for (int i = _boardRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(_boardRoot.GetChild(i).gameObject);
            }
        }
        /// <summary>Plays the "a gecko just left here" feedback on one exit hole.</summary>
        public void PlayExitFeedback(GridPosition cell)
        {
            if (_exitViews.TryGetValue(cell, out ExitView exitView))
            {
                exitView.PlayPulse();
            }

            if (_exitParticlePrefab != null)
            {
                ParticleSystem particles = Instantiate(_exitParticlePrefab,
                    Layout.CellToWorld(cell), _exitParticlePrefab.transform.rotation);
                particles.Play();
                Destroy(particles.gameObject, 2f);
            }
        }
    }
}