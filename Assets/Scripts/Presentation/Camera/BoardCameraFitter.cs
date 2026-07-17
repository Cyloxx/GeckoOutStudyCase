using UnityEngine;

namespace GeckoOut.Presentation.Cameras
{
    /// <summary>Sizes the orthographic camera so the whole board is visible.</summary>
    public class BoardCameraFitter : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _padding = 1f;

        public void Fit(int boardWidth, int boardHeight, float cellSize)
        {
            float halfBoardHeight = boardHeight * cellSize * 0.5f + _padding;
            float halfBoardWidth = boardWidth * cellSize * 0.5f + _padding;

            // Orthographic size is the half-height of the view; width is
            // covered by dividing the needed half-width by the aspect ratio.
            float sizeForHeight = halfBoardHeight;
            float sizeForWidth = halfBoardWidth / _camera.aspect;

            _camera.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);
        }
    }
}