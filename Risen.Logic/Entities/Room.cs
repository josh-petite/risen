using System.Collections.Generic;
using Risen.Server.Enums;
using Risen.Server.Utility;

namespace Risen.Server.Entities
{
    public class Room
    {
        public Room()
        {
        }

        public Room(uint id)
        {
            Id = id;
        }

        public uint Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Player> PlayersPresent { get; set; }
        public Dictionary<Direction, Room> Exits { get; set; }
        public Zone Zone { get; set; }
        public Point Coordinates { get; set; }
    }
}
