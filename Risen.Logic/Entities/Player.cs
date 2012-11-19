using System.Collections.Generic;

namespace Risen.Logic.Entities
{
    public class Player
    {
        public Player(Room spawnRoom)
        {
            CurrentRoom = spawnRoom;
        }

        public string GivenName { get; set; }
        public string Surname { get; set; }
        public CharacterClass Class  { get; set; }
        public byte Level { get; set; }
        public Room CurrentRoom { get; set; }
    }

    public class CharacterClass
    {
        public string Name { get; set; }
        public ExperienceTable ExperienceTable { get; set; }
    }

    public class ExperienceTable
    {
        public Dictionary<byte, int> LevelDefinitions { get; set; } 
    }
}
