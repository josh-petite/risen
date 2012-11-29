using Risen.Server.Enums;

namespace Risen.Server.Entities
{
    public class RoomExit : EntityBase
    {
        public virtual Exit Exit { get; set; }
        public virtual Room SourceRoom { get; set; }
        public virtual Room DestinationRoom { get; set; }
    }
}
