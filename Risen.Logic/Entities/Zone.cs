using System.Collections.Generic;
using System.Linq;

namespace Risen.Server.Entities
{
    public class Zone : EntityBase
    {
        public virtual string Name { get; set; }
        public virtual List<Player> ActivePlayers { get; set; }
        public virtual IEnumerable<Room> Rooms { get; set; }

        public virtual Room GetRoom(long roomId)
        {
            return Rooms.Single(o => o.Id == Id);
        }

        public virtual Room GetSpawnRoom()
        {
            return Rooms.Any() ? Rooms.First() : null;
        }
    }
}
