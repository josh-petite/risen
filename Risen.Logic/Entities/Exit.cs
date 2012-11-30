using Risen.Server.ReferenceTypes;

namespace Risen.Server.Entities
{
    public class Exit : EntityBase
    {
        public virtual ExitTemplate ExitTemplate { get; set; }
        public virtual string CustomDescription { get; set; }
    }
}
