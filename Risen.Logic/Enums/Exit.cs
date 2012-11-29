using Risen.Server.Entities;
using Risen.Server.ReferenceTypes;

namespace Risen.Server.Enums
{
    public class Exit : EntityBase
    {
        public virtual ExitTemplateRef ExitTemplate { get; set; }
        public virtual string CustomDescription { get; set; }
    }
}
