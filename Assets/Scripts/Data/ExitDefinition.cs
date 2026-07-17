using System;

namespace GeckoOut.Data
{
    /// <summary>JSON shape of one exit hole.</summary>
    [Serializable]
    public class ExitDefinition
    {
        public int x;
        public int y;
        public string color;
    }
}