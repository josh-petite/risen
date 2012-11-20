using Risen.Server.Entities;

namespace Risen.Server.Logic
{
    public interface IZoneLogic
    {
        Zone CacheZone(uint zoneId);
        Room GetRoom(Zone zone, uint roomId);
    }

    public class ZoneLogic : IZoneLogic
    {
        public Zone CacheZone(uint zoneId)
        {
            return new Zone();
        }

        public Room GetRoom(Zone zone, uint roomId)
        {
            return zone.GetRoom(roomId);
        }
    }
}
