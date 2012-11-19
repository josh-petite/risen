using System.Collections.Generic;
using System.Linq;

namespace Risen.Logic.Entities
{
    public class Zone
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public List<Player> ActivePlayers { get; set; }
        public Map Map { get; set; }
        
        public Room GetRoom(uint roomId)
        {
            return Map.Rooms.Single(o => o.Id == Id);
        }
    }

    public class Map
    {
        public IList<Room> Rooms { get; set; }
    }
}
