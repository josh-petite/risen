using System.Collections.Generic;
using System.Linq;

namespace Risen.Logic.Entities
{
    public class Zone
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public List<Player> ActivePlayers { get; set; }
        public IEnumerable<Room> Rooms { get; set; }
        
        public Room GetRoom(uint roomId)
        {
            return Rooms.Single(o => o.Id == roomId);
        }
    }
}
