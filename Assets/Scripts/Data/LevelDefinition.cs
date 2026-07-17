using System;
using System.Collections.Generic;

namespace GeckoOut.Data
{
    /// <summary>Root JSON shape of one level file.</summary>
    [Serializable]
    public class LevelDefinition
    {
        public int levelId;
        public float timeLimitSeconds;
        public int gridWidth;
        public int gridHeight;
        public List<CellDefinition> walls = new List<CellDefinition>();
        public List<ExitDefinition> exits = new List<ExitDefinition>();
        public List<GeckoDefinition> geckos = new List<GeckoDefinition>();
    }
}