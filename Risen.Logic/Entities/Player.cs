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

        public PlayerClassReferenceType ClassReferenceType { get; set; }
        public byte Level { get; set; }
        public long Experience { get; set; }
        public Attributes Attributes { get; set; }
    }
}
