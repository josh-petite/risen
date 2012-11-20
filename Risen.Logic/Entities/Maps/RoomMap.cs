using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class RoomMap : ClassMap<Room>
    {
        public RoomMap()
        {
            Id(o => o.Id);
            Map(o => o.Name);
            Map(o => o.Zone);
            Map(o => o.Exits);
        }
    }
}
