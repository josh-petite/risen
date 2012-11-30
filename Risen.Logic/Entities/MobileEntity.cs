using System.Linq;
using Risen.Server.Caches;
using Risen.Server.Enums;
using Risen.Server.Utility;

namespace Risen.Server.Entities
{
    public class MobileEntity : EntityBase
    {
        public virtual Title Title { get; set; }
        public virtual string Name { get; set; }
        public virtual string Surname { get; set; }
        public virtual PostTitle PostTitle { get; set; }
        public virtual Room CurrentRoom { get; set; }

        public virtual void MoveTo(Exit exit)
        {
            if (!CurrentRoom.RoomExits.Select(o => o.Exit).Contains(exit))
                return;

            CurrentRoom = CurrentRoom.RoomExits.Single(o => o.Exit == exit).DestinationRoom;
        }

        public virtual Room MoveTo(Point roomCoordinates)
        {
            CurrentRoom = ZoneCache.MoveMobileEntityTo(this, roomCoordinates);
            return CurrentRoom;
        }
    }
}