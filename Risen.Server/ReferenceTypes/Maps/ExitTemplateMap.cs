using FluentNHibernate.Mapping;

namespace Risen.Server.ReferenceTypes.Maps
{
    public class ExitTemplateMap : ClassMap<ExitTemplate>
    {
        public ExitTemplateMap()
        {
            Table("ExitTemplates");
            Id(o => o.Id, "ExitTemplateId");
            Map(o => o.Description);
        }
    }
}