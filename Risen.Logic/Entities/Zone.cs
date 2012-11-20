using System.Collections.Generic;
using System.Linq;

namespace Risen.Logic.Entities
{
    public class Zone
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public List<Player> ActivePlayers { get; set; }
        public IList<Room> Rooms { get; set; }
        
        public Room GetRoom(uint roomId)
        {
            return Rooms.Single(o => o.Id == Id);
        }

        public Room GetSpawnRoom()
        {
            return Rooms.Any() ? Rooms.First() : null;
        }
    }
}
