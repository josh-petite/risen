using System.Collections.Generic;
using Risen.Server.Enums;
using Risen.Server.Utility;

namespace Risen.Server.Entities
{
    public class Room : EntityBase
    {
        public Room()
        {
        }

        public virtual string Name { get; set; }
        public virtual IEnumerable<Player> PlayersPresent { get; set; }
        public virtual Dictionary<Direction, Room> Exits { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual Point Coordinates { get; set; }
    }
}
