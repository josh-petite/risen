using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class RoomExitMap : ClassMap<RoomExit>
    {
        public RoomExitMap()
        {
            Id(o => o.Id, "RoomExitId");
            References(o => o.Exit, "ExitId");
            References(o => o.SourceRoom, "SourceRoomId");
            References(o => o.DestinationRoom, "DestinationRoomId");
        }
    }
}
