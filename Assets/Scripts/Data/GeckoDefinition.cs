using System;
using System.Collections.Generic;

namespace GeckoOut.Data
{
    /// <summary>JSON shape of one gecko: color + ordered cells, head first.</summary>
    [Serializable]
    public class GeckoDefinition
    {
        public string color;
        public List<CellDefinition> cells = new List<CellDefinition>();
    }
}