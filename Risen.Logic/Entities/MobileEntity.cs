using System.Linq;
using Risen.Server.Enums;
using Risen.Server.Utility;

namespace Risen.Server.Entities
{
    public class MobileEntity : EntityBase
    {
        public Title Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public PostTitle PostTitle { get; set; }
        public Room CurrentRoom { get; set; }

        public void MoveTo(Exit exit)
        {
            if (!CurrentRoom.RoomExits.Select(o => o.Exit).Contains(exit))
                return;

            CurrentRoom = CurrentRoom.RoomExits.Single(o => o.Exit == exit).DestinationRoom;
        }

        public Room MoveTo(Point roomCoordinates)
        {
            CurrentRoom = ZoneCache.MoveMobileEntityTo(this, roomCoordinates);
            return CurrentRoom;
        }
    }
}