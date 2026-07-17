using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;

namespace GeckoOut.Data
{
    /// <summary>Everything the factory produced from one level file.</summary>
    public class LoadedLevel
    {
        public int LevelId { get; }
        public float TimeLimitSeconds { get; }
        public BoardGrid Board { get; }
        public List<GeckoBody> Geckos { get; }

        public LoadedLevel(int levelId, float timeLimitSeconds,
            BoardGrid board, List<GeckoBody> geckos)
        {
            LevelId = levelId;
            TimeLimitSeconds = timeLimitSeconds;
            Board = board;
            Geckos = geckos;
        }
    }
}