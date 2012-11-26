using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class RoomMap : ClassMap<Room>
    {
        public RoomMap()
        {
            Id(o => o.Id).Column("RoomId");
            Map(o => o.Name).Not.Nullable();
            Map(o => o.Zone).Not.Nullable();
            Map(o => o.Exits).Not.Nullable();
            Component(c => c.Coordinates, m =>
                                              {
                                                  m.Map(o => o.X).Column("CoordinateX");
                                                  m.Map(o => o.Y).Column("CoordinateY");
                                              });
        }
    }
}
