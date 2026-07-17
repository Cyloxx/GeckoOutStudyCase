using System;
using UnityEngine;

namespace GeckoOut.Data
{
    /// <summary>
    /// Parses level JSON text into a LevelDefinition.
    /// Only parsing lives here; rule checks belong to LevelValidator.
    /// </summary>
    public class LevelDefinitionLoader
    {
        public LevelDefinition Load(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Level JSON is null or empty.");
            }

            LevelDefinition definition;

            try
            {
                definition = JsonUtility.FromJson<LevelDefinition>(json);
            }
            catch (Exception exception)
            {
                throw new ArgumentException(
                    "Level JSON could not be parsed: " + exception.Message, exception);
            }

            if (definition == null)
            {
                throw new ArgumentException("Level JSON produced no data.");
            }

            return definition;
        }
    }
}