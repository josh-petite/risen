using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class RoomMap : ClassMap<Room>
    {
        public RoomMap()
        {
            Table("Rooms");
            Id(o => o.Id, "RoomId");
            Map(o => o.Name).Not.Nullable();
            Map(o => o.Zone).Not.Nullable();
            Component(c => c.Coordinates, m =>
                                              {
                                                  m.Map(o => o.X).Column("CoordinateX");
                                                  m.Map(o => o.Y).Column("CoordinateY");
                                              });
            Join("RoomExits", o => o.Map(x => x.Exits));
        }
    }
}
