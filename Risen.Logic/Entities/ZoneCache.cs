using System.Collections.Generic;
using System.Linq;
using Risen.Logic.Enums;
using Risen.Logic.Utility;

namespace Risen.Logic.Entities
{
    public class ZoneCache
    {
        static ZoneCache()
        {
            Cache = new List<Zone>();
        }

        public static List<Zone> Cache { get; set; }

        public void LoadZoneIntoCache(uint zoneId)
        {
            Cache.Add(new Zone()); // call fluent for value in the future
        }

        public static Room MovePlayerTo(Player player, Point roomCoordinates)
        {
            var targetRoom = player.CurrentRoom.Zone.Rooms.First(o => o.Coordinates == roomCoordinates);
            player.CurrentRoom = targetRoom;

            return targetRoom;
        }
    }
}