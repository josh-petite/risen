using System.Collections.Generic;
using System.Linq;

namespace Risen.Server.Entities
{
    public class Zone : EntityBase
    {
        public string Name { get; set; }
        public List<Player> ActivePlayers { get; set; }
        public IList<Room> Rooms { get; set; }

        public Room GetRoom(long roomId)
        {
            return Rooms.Single(o => o.Id == Id);
        }

        public Room GetSpawnRoom()
        {
            return Rooms.Any() ? Rooms.First() : null;
        }
    }
}
