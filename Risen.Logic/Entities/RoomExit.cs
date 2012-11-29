using Risen.Server.Enums;

namespace Risen.Server.Entities
{
    public class RoomExit : EntityBase
    {
        public virtual Direction Direction { get; set; }
        public virtual Room SourceRoom { get; set; }
        public virtual Room DestinationRoom { get; set; }
    }
}
