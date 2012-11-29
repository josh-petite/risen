using System.Linq;
using Risen.Server.Enums;
using Risen.Server.ReferenceTypes;
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

        public void MoveTo(Direction exit)
        {
            if (CurrentRoom.Exits.All(o => o.Direction != exit))
                return;

            CurrentRoom = CurrentRoom.Exits.Single(o => o.Direction == exit).DestinationRoom;
        }

        public Room MoveTo(Point roomCoordinates)
        {
            CurrentRoom = ZoneCache.MoveMobileEntityTo(this, roomCoordinates);
            return CurrentRoom;
        }
    }
}