using FluentNHibernate.Mapping;

namespace Risen.Server.ReferenceTypes.Maps
{
    public class PlayerClassMap : ClassMap<PlayerClass>
    {
        public PlayerClassMap()
        {
            Table("PlayerClasses");
            Id(o => o.Id, "PlayerClassId");
            Map(o => o.Name);
        }
    }
}
