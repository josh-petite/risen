using System.Collections.Generic;

namespace Risen.Server.ReferenceTypes
{
    public class LevelDefinitionReferenceType
    {
        public Dictionary<byte, int> Definitions { get; set; } 

        public int GetExperienceNeededUntilNextLevel(byte currentLevel, int currentExperience)
        {
            var nextLevel = (byte) (currentLevel + 1);

            if (!Definitions.ContainsKey(nextLevel))
                return 0;

            return Definitions[nextLevel] - currentExperience;
        }
    }
}
