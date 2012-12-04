using System.Collections.Generic;
using System.Linq;
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
        public virtual IEnumerable<RoomExit> RoomExits { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual Point Coordinates { get; set; }

        public virtual Room GetRoomInDirectionOf(int exitTemplateId)
        {
            var roomExit = RoomExits.SingleOrDefault(o => o.Exit.ExitTemplate.Id == exitTemplateId);
            return roomExit != null ? roomExit.DestinationRoom : null;
        }
    }
}
