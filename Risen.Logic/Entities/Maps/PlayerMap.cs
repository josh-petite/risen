using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class PlayerMap : ClassMap<Player>
    {
        public PlayerMap()
        {
            Id(o => o.Id, "PlayerId");
            Component(c => c.ClassReferenceType, m =>
                                        {
                                            m.Map(o => o.ImageId);
                                            m.Map(o => o.Name);
                                        });
        }
    }
}
