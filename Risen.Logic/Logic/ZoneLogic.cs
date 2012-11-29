using Risen.Server.Entities;

namespace Risen.Server.Logic
{
    public interface IZoneLogic
    {
        Zone CacheZone(long zoneId);
        Room GetRoom(Zone zone, long roomId);
    }

    public class ZoneLogic : IZoneLogic
    {
        public Zone CacheZone(long zoneId)
        {
            return new Zone();
        }

        public Room GetRoom(Zone zone, long roomId)
        {
            return zone.GetRoom(roomId);
        }
    }
}
