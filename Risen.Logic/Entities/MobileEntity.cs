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

        public void MoveTo(Direction exit)
        {
            if (!CurrentRoom.Exits.ContainsKey(exit))
                return;

            CurrentRoom = CurrentRoom.Exits[exit];
        }

        public Room MoveTo(Point roomCoordinates)
        {
            CurrentRoom = ZoneCache.MoveMobileEntityTo(this, roomCoordinates);
            return CurrentRoom;
        }
    }
}