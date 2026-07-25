using UnityEditor;
using UnityEngine;

namespace GeckoOut.Editor
{
    /// <summary>
    /// Visual level designer. Paints a board on a grid and (from C.4) exports
    /// it as the same JSON the game loads. All rules come from the existing
    /// Data layer — the editor only draws and collects cells.
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        private enum CellKind
        {
            Empty = 0,
            Wall = 1,
            Exit = 2
        }

        private const int MinSize = 3;
        private const int MaxSize = 14;
        private const float CellPixels = 34f;

        private int _width = 6;
        private int _height = 6;
        private CellKind[,] _cells;

        [MenuItem("Tools/GeckoOut/Level Editor")]
        public static void Open()
        {
            LevelEditorWindow window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Level Editor");
            window.minSize = new Vector2(560f, 680f);
            window.Show();
        }

        private void OnEnable()
        {
            EnsureGrid();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GeckoOut Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSizeControls();
            EditorGUILayout.Space();
            DrawGrid();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Click a cell to cycle: Empty -> Wall -> Exit. Geckos come next.",
                MessageType.Info);
        }

        private void DrawSizeControls()
        {
            EditorGUILayout.BeginHorizontal();

            int newWidth = EditorGUILayout.IntSlider("Width", _width, MinSize, MaxSize);
            int newHeight = EditorGUILayout.IntSlider("Height", _height, MinSize, MaxSize);

            EditorGUILayout.EndHorizontal();

            if (newWidth != _width || newHeight != _height)
            {
                _width = newWidth;
                _height = newHeight;
                ResizeGrid();
            }

            if (GUILayout.Button("Clear"))
            {
                _cells = new CellKind[_width, _height];
            }
        }

        private void DrawGrid()
        {
            // Rows are drawn top-down so y increases upwards, matching the game.
            for (int y = _height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < _width; x++)
                {
                    DrawCellButton(x, y);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawCellButton(int x, int y)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = ColorFor(_cells[x, y]);

            if (GUILayout.Button(LabelFor(_cells[x, y]),
                    GUILayout.Width(CellPixels), GUILayout.Height(CellPixels)))
            {
                CycleCell(x, y);
            }

            GUI.backgroundColor = previous;
        }

        private void CycleCell(int x, int y)
        {
            CellKind current = _cells[x, y];
            int next = ((int)current + 1) % 3;
            _cells[x, y] = (CellKind)next;
        }

        private Color ColorFor(CellKind kind)
        {
            switch (kind)
            {
                case CellKind.Wall: return new Color(0.35f, 0.35f, 0.38f);
                case CellKind.Exit: return new Color(0.35f, 0.7f, 1f);
                default: return new Color(0.85f, 0.85f, 0.85f);
            }
        }

        private string LabelFor(CellKind kind)
        {
            switch (kind)
            {
                case CellKind.Wall: return "W";
                case CellKind.Exit: return "E";
                default: return "";
            }
        }

        private void EnsureGrid()
        {
            if (_cells == null || _cells.GetLength(0) != _width || _cells.GetLength(1) != _height)
            {
                _cells = new CellKind[_width, _height];
            }
        }

        private void ResizeGrid()
        {
            CellKind[,] resized = new CellKind[_width, _height];

            int copyWidth = Mathf.Min(_width, _cells.GetLength(0));
            int copyHeight = Mathf.Min(_height, _cells.GetLength(1));

            for (int x = 0; x < copyWidth; x++)
            {
                for (int y = 0; y < copyHeight; y++)
                {
                    resized[x, y] = _cells[x, y];
                }
            }

            _cells = resized;
        }
    }
}