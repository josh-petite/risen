using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class PlayerMap : ClassMap<Player>
    {
        public PlayerMap()
        {
            Id(o => o.Id, "PlayerId");
            Component(c => c.Class, m =>
                                        {
                                            //m.Map(o => o.ImageId);
                                            m.Map(o => o.Name);
                                        });
            Component(c => c.Attributes, a =>
                                             {
                                                 a.Map(o => o.Agility);
                                                 a.Map(o => o.Aptitude);
                                                 a.Map(o => o.Aura);
                                                 a.Map(o => o.Charisma);
                                                 a.Map(o => o.Endurance);
                                                 a.Map(o => o.Faith);
                                                 a.Map(o => o.Ingenuity);
                                                 a.Map(o => o.Memory);
                                                 a.Map(o => o.Reflex);
                                                 a.Map(o => o.Strength);
                                                 a.Map(o => o.Willpower);
                                                 a.Map(o => o.Wisdom);
                                             });
        }
    }
}
