using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Presentation.Common;
using UnityEditor;
using UnityEngine;
using System.IO;
using GeckoOut.Data;

namespace GeckoOut.Editor
{
    /// <summary>
    /// Visual level designer. Paints walls, colored exits and multi-cell
    /// geckos on a grid and (from C.4) exports the same JSON the game loads.
    /// All rules come from the Data layer — the editor only draws and collects.
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        private enum Tool
        {
            Wall = 0,
            Exit = 1,
            Gecko = 2,
            Erase = 3
        }

        private class GeckoDraft
        {
            public ColorId Color;
            public readonly List<Vector2Int> Cells = new List<Vector2Int>();
        }

        private const int MinSize = 3;
        private const int MaxSize = 14;
        private const float CellPixels = 34f;

        private int _width = 6;
        private int _height = 6;
        private int _levelId = 1;
        private float _timeLimitSeconds = 30f;

        private Tool _tool = Tool.Wall;
        private ColorId _selectedColor = ColorId.Red;

        private readonly HashSet<Vector2Int> _walls = new HashSet<Vector2Int>();
        private readonly Dictionary<Vector2Int, ColorId> _exits = new Dictionary<Vector2Int, ColorId>();
        private readonly List<GeckoDraft> _geckos = new List<GeckoDraft>();

        [MenuItem("Tools/GeckoOut/Level Editor")]
        public static void Open()
        {
            LevelEditorWindow window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Level Editor");
            window.minSize = new Vector2(560f, 720f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GeckoOut Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSettings();
            EditorGUILayout.Space();
            DrawToolbar();
            EditorGUILayout.Space();
            DrawGrid();
            EditorGUILayout.Space();
            DrawValidationAndExport();
            EditorGUILayout.Space();
            DrawHelp();
        }

        private void DrawSettings()
        {
            _levelId = EditorGUILayout.IntField("Level Id", _levelId);
            _timeLimitSeconds = EditorGUILayout.FloatField("Time Limit (s)", _timeLimitSeconds);

            int newWidth = EditorGUILayout.IntSlider("Width", _width, MinSize, MaxSize);
            int newHeight = EditorGUILayout.IntSlider("Height", _height, MinSize, MaxSize);

            if (newWidth != _width || newHeight != _height)
            {
                _width = newWidth;
                _height = newHeight;
                DropOutOfBounds();
            }
        }

        private void DrawToolbar()
        {
            _tool = (Tool)GUILayout.Toolbar((int)_tool,
                new[] { "Wall", "Exit", "Gecko", "Erase" });

            if (_tool == Tool.Exit || _tool == Tool.Gecko)
            {
                _selectedColor = (ColorId)EditorGUILayout.EnumPopup("Color", _selectedColor);

                Rect swatch = GUILayoutUtility.GetRect(40f, 16f);
                EditorGUI.DrawRect(swatch, ColorPalette.ToUnityColor(_selectedColor));
            }

            EditorGUILayout.BeginHorizontal();

            if (_tool == Tool.Gecko && GUILayout.Button("New Gecko"))
            {
                StartNewGecko();
            }

            if (GUILayout.Button("Clear All"))
            {
                _walls.Clear();
                _exits.Clear();
                _geckos.Clear();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawGrid()
        {
            for (int y = _height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();

                for (int x = 0; x < _width; x++)
                {
                    DrawCellButton(new Vector2Int(x, y));
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawCellButton(Vector2Int cell)
        {
            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = ColorForCell(cell);

            if (GUILayout.Button(LabelForCell(cell),
                    GUILayout.Width(CellPixels), GUILayout.Height(CellPixels)))
            {
                HandleClick(cell);
            }

            GUI.backgroundColor = previous;
        }

        private void HandleClick(Vector2Int cell)
        {
            switch (_tool)
            {
                case Tool.Wall:
                    PaintWall(cell);
                    break;
                case Tool.Exit:
                    PaintExit(cell);
                    break;
                case Tool.Gecko:
                    PaintGecko(cell);
                    break;
                case Tool.Erase:
                    EraseCell(cell);
                    break;
            }
        }

        private void PaintWall(Vector2Int cell)
        {
            if (IsExit(cell) || GeckoAt(cell) != null)
            {
                return;
            }

            if (!_walls.Add(cell))
            {
                _walls.Remove(cell);
            }
        }

        private void PaintExit(Vector2Int cell)
        {
            if (_walls.Contains(cell) || GeckoAt(cell) != null)
            {
                return;
            }

            _exits[cell] = _selectedColor;
        }

        private void PaintGecko(Vector2Int cell)
        {
            if (_walls.Contains(cell) || IsExit(cell) || GeckoAt(cell) != null)
            {
                return;
            }

            GeckoDraft active = ActiveGecko();

            if (active == null)
            {
                StartNewGecko();
                active = ActiveGecko();
            }

            if (active.Cells.Count > 0 && !IsAdjacent(active.Cells[active.Cells.Count - 1], cell))
            {
                return;
            }

            active.Color = _selectedColor;
            active.Cells.Add(cell);
        }

        private void EraseCell(Vector2Int cell)
        {
            _walls.Remove(cell);
            _exits.Remove(cell);

            GeckoDraft gecko = GeckoAt(cell);
            if (gecko != null)
            {
                _geckos.Remove(gecko);
            }
        }

        private void StartNewGecko()
        {
            _geckos.Add(new GeckoDraft { Color = _selectedColor });
        }

        private GeckoDraft ActiveGecko()
        {
            if (_geckos.Count == 0)
            {
                return null;
            }

            return _geckos[_geckos.Count - 1];
        }

        private GeckoDraft GeckoAt(Vector2Int cell)
        {
            foreach (GeckoDraft gecko in _geckos)
            {
                if (gecko.Cells.Contains(cell))
                {
                    return gecko;
                }
            }

            return null;
        }

        private bool IsExit(Vector2Int cell)
        {
            return _exits.ContainsKey(cell);
        }

        private bool IsAdjacent(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) == 1;
        }

        private Color ColorForCell(Vector2Int cell)
        {
            if (_walls.Contains(cell))
            {
                return new Color(0.35f, 0.35f, 0.38f);
            }

            if (_exits.TryGetValue(cell, out ColorId exitColor))
            {
                return ColorPalette.ToUnityColor(exitColor);
            }

            GeckoDraft gecko = GeckoAt(cell);
            if (gecko != null)
            {
                return ColorPalette.ToUnityColor(gecko.Color);
            }

            return new Color(0.85f, 0.85f, 0.85f);
        }

        private string LabelForCell(Vector2Int cell)
        {
            if (_walls.Contains(cell))
            {
                return "W";
            }

            if (IsExit(cell))
            {
                return "E";
            }

            GeckoDraft gecko = GeckoAt(cell);
            if (gecko != null)
            {
                return gecko.Cells[0] == cell ? "H" : "o";
            }

            return "";
        }

        private void DropOutOfBounds()
        {
            _walls.RemoveWhere(cell => !InBounds(cell));

            var staleExits = new List<Vector2Int>();
            foreach (KeyValuePair<Vector2Int, ColorId> pair in _exits)
            {
                if (!InBounds(pair.Key))
                {
                    staleExits.Add(pair.Key);
                }
            }
            foreach (Vector2Int cell in staleExits)
            {
                _exits.Remove(cell);
            }

            _geckos.RemoveAll(gecko => gecko.Cells.Exists(cell => !InBounds(cell)));
        }

        private bool InBounds(Vector2Int cell)
        {
            return cell.x >= 0 && cell.x < _width && cell.y >= 0 && cell.y < _height;
        }

        private void DrawHelp()
        {
            EditorGUILayout.HelpBox(
                "Wall/Exit/Erase: click a cell. Gecko: click cells in order (head "
                + "first) to build a chain; 'New Gecko' starts another. Export comes next.",
                MessageType.Info);
        }
                private void DrawValidationAndExport()
        {
            LevelDefinition definition = BuildDefinition();
            bool valid = new LevelValidator().IsValid(definition, out List<string> errors);

            if (valid)
            {
                EditorGUILayout.HelpBox("Level is valid and ready to export.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(string.Join("\n", errors), MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(!valid);

            if (GUILayout.Button("Export JSON", GUILayout.Height(30f)))
            {
                Export(definition);
            }

            EditorGUI.EndDisabledGroup();
        }

        private LevelDefinition BuildDefinition()
        {
            LevelDefinition definition = new LevelDefinition();
            definition.levelId = _levelId;
            definition.timeLimitSeconds = _timeLimitSeconds;
            definition.gridWidth = _width;
            definition.gridHeight = _height;

            foreach (Vector2Int wall in _walls)
            {
                definition.walls.Add(new CellDefinition { x = wall.x, y = wall.y });
            }

            foreach (KeyValuePair<Vector2Int, ColorId> exit in _exits)
            {
                definition.exits.Add(new ExitDefinition
                {
                    x = exit.Key.x,
                    y = exit.Key.y,
                    color = exit.Value.ToString()
                });
            }

            foreach (GeckoDraft gecko in _geckos)
            {
                GeckoDefinition geckoDefinition = new GeckoDefinition
                {
                    color = gecko.Color.ToString()
                };

                foreach (Vector2Int cell in gecko.Cells)
                {
                    geckoDefinition.cells.Add(new CellDefinition { x = cell.x, y = cell.y });
                }

                definition.geckos.Add(geckoDefinition);
            }

            return definition;
        }

        private void Export(LevelDefinition definition)
        {
            string defaultName = "Level_" + _levelId.ToString("00") + ".json";
            string path = EditorUtility.SaveFilePanelInProject(
                "Export Level", defaultName, "json", "Choose where to save the level");

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            File.WriteAllText(path, JsonUtility.ToJson(definition, true));
            AssetDatabase.Refresh();
            Debug.Log("[LevelEditor] Exported " + path);
        }
    }
}