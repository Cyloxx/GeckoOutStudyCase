using GeckoOut.Core.Board;
using UnityEngine;

namespace GeckoOut.Presentation.Board
{
    /// <summary>
    /// Single source of truth for grid-to-world conversion. Cells lie on
    /// the XZ plane and the whole board is centered on the world origin.
    /// </summary>
    public class BoardLayout
    {
        public float CellSize { get; }

        private readonly float _originX;
        private readonly float _originZ;

        public BoardLayout(int width, int height, float cellSize)
        {
            CellSize = cellSize;
            _originX = -(width - 1) * 0.5f * cellSize;
            _originZ = -(height - 1) * 0.5f * cellSize;
        }

        public Vector3 CellToWorld(GridPosition cell)
        {
            return new Vector3(
                _originX + cell.X * CellSize,
                0f,
                _originZ + cell.Y * CellSize);
        }

        public GridPosition WorldToCell(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt((worldPosition.x - _originX) / CellSize);
            int y = Mathf.RoundToInt((worldPosition.z - _originZ) / CellSize);
            return new GridPosition(x, y);
        }
    }
}