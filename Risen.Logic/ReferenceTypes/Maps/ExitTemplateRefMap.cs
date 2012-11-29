using FluentNHibernate.Mapping;

namespace Risen.Server.ReferenceTypes.Maps
{
    public class ExitTemplateRefMap : ClassMap<ExitTemplateRef>
    {
        public ExitTemplateRefMap()
        {
            Id(o => o.Id, "ExitTemplateRefId");
            Map(o => o.Description);
        }
    }
}