using System.Collections.Generic;
using System.Linq;

namespace Risen.Server.Entities
{
    public class World
    {
        public List<Zone> Zones { get; set; }

        public IEnumerable<Player> AllPlayersOnline
        {
            get { return Zones.SelectMany(o => o.ActivePlayers); }
        }
    }
}