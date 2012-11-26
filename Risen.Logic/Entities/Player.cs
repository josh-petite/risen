using Risen.Server.Enums;
using Risen.Server.Utility;

namespace Risen.Server.Entities
{
    public sealed class Player : MobileEntity
    {
        public Player()
        {
        }

        public Player(Room spawnRoom)
        {
            CurrentRoom = spawnRoom;
        }

        public Title Title { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public PostTitle PostTitle { get; set; }
        public PlayerClassReferenceType ClassReferenceType { get; set; }
        public byte Level { get; set; }
        public Room CurrentRoom { get; set; }
        
        public void MoveTo(Direction exit)
        {
            if (!CurrentRoom.Exits.ContainsKey(exit))
                return;

            CurrentRoom = CurrentRoom.Exits[exit];
        }

        public Room MoveTo(Point roomCoordinates)
        {
            CurrentRoom = ZoneCache.MovePlayerTo(this, roomCoordinates);
            return CurrentRoom;
        }
    }
}
