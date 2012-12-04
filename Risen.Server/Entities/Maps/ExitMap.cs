using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class ExitMap : ClassMap<Exit>
    {
        public ExitMap()
        {
            Table("Exits");
            Id(o => o.Id, "ExitId");
            References(o => o.ExitTemplate);
            Map(o => o.CustomDescription);
        }
    }
}
