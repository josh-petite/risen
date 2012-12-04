using FluentNHibernate.Mapping;

namespace Risen.Server.Entities.Maps
{
    public class CommentMap : ClassMap<Comment>
    {
        public CommentMap()
        {
            Id(o => o.Id, "CommentId");
            Map(o => o.Text);
        }
    }
}
