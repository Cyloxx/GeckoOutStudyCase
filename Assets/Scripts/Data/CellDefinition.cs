using System;

namespace GeckoOut.Data
{
    /// <summary>JSON shape of a single grid cell reference.</summary>
    [Serializable]
    public class CellDefinition
    {
        public int x;
        public int y;
    }
}