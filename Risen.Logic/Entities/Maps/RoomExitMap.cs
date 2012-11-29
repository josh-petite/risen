using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class RoomExitMap : ClassMap<RoomExit>
    {
        public RoomExitMap()
        {
            Id(o => o.Id, "RoomExitId");
            Map(o => o.Exit, "DirectionId");
            Map(o => o.SourceRoom, "RoomId");
            Map(o => o.DestinationRoom, "DestinationRoomId");
        }
    }
}
