using System.Collections.Generic;
using System.Linq;
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
        public virtual string Description { get; set; }
        public virtual IEnumerable<Player> PlayersPresent { get; set; }
        public virtual IEnumerable<RoomExit> Exits { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual Point Coordinates { get; set; }

        public Room GetRoomInDirectionOf(Direction direction)
        {
            var roomExit = Exits.SingleOrDefault(o => o.Direction == direction);
            return roomExit != null ? roomExit.DestinationRoom : null;
        }
    }
}
