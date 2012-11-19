using System.Collections.Generic;
using Risen.Logic.Enums;

namespace Risen.Logic.Entities
{
    public class Room
    {
        public Room(uint id)
        {
            Id = id;
        }

        public uint Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Player> PlayersPresent { get; set; }
        public Zone Zone { get; set; }
        public Dictionary<Direction, uint> Exits { get; set; } 

        public bool ExitExists(Direction direction)
        {
            return Exits
        }
    }
}
