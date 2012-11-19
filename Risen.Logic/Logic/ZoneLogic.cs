using Risen.Logic.Entities;
using Risen.Logic.Enums;

namespace Risen.Logic.Logic
{
    public interface IZoneLogic
    {
        Zone CacheZone(uint zoneId);
        Room GetRoom(Zone zone, uint roomId);
        void MovePlayer(Player player, Direction direction);
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

        public void MovePlayer(Player player, Direction direction)
        {
            if (player.CurrentRoom.ExitExists(direction))
                player.CurrentRoom = 
        }
    }
}
