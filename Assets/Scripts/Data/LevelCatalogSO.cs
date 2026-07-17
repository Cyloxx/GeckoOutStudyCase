using System.Collections.Generic;
using UnityEngine;

namespace GeckoOut.Data
{
    /// <summary>Ordered list of level JSON files, editable in the inspector.</summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "GeckoOut/Level Catalog")]
    public class LevelCatalogSO : ScriptableObject
    {
        [SerializeField] private List<TextAsset> _levelFiles = new List<TextAsset>();

        public int LevelCount
        {
            get { return _levelFiles.Count; }
        }

        public TextAsset GetLevelFile(int index)
        {
            if (index < 0 || index >= _levelFiles.Count)
            {
                return null;
            }

            return _levelFiles[index];
        }
    }
}