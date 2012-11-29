using FluentNHibernate.Mapping;
using Risen.Server.Enums;

namespace Risen.Server.Entities.Maps
{
    public class ExitMap : ClassMap<Exit>
    {
        public ExitMap()
        {
            Id(o => o.Id, "ExitId");
            Map(o => o.ExitTemplate);
            Map(o => o.CustomDescription);
        }
    }
}
