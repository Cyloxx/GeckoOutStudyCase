using UnityEditor;
using UnityEngine;

namespace GeckoOut.Editor
{
    /// <summary>
    /// Visual level designer. Paints a board on a grid and exports it as the
    /// same JSON the game loads at runtime. All rules come from the existing
    /// Data layer (LevelValidator / LevelDefinition) — the editor only draws.
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        [MenuItem("Tools/GeckoOut/Level Editor")]
        public static void Open()
        {
            LevelEditorWindow window = GetWindow<LevelEditorWindow>();
            window.titleContent = new GUIContent("Level Editor");
            window.minSize = new Vector2(520f, 640f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("GeckoOut Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Grid painting and JSON export will be added in the next steps.",
                MessageType.Info);
        }
    }
}