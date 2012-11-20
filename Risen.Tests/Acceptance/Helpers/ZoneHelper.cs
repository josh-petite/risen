using System.Collections.Generic;
using System.Linq;
using Risen.Logic.Entities;
using Risen.Logic.Enums;
using Risen.Logic.Utility;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Helpers
{
    public static class ZoneHelper
    {
        public static Zone GetZoneFromContext()
        {
            return (Zone)ScenarioContext.Current.Single(o => o.Value.GetType() == typeof(Zone)).Value;
        }

        public static Zone BuildMapFromMapType(string mapType)
        {
            switch(mapType.ToLower())
            {
                case "cube":
                    return BuildCubeZone();
                case "north to south hallway":
                    return NorthToSouthHallway();
                default:
                    return new Zone();
            }
        }

        private static Zone NorthToSouthHallway()
        {
            var zone = CreateEmptyZone(3, 5);

            var room00 = new Room(0) { Coordinates = new Point(0, 0), Zone = zone };
            var room01 = new Room(1) { Coordinates = new Point(0, 1), Zone = zone };
            var room02 = new Room(2) { Coordinates = new Point(0, 2), Zone = zone };
            var room10 = new Room(3) { Coordinates = new Point(1, 0), Zone = zone };
            var room11 = new Room(4) { Coordinates = new Point(1, 1), Zone = zone };
            var room12 = new Room(5) { Coordinates = new Point(1, 2), Zone = zone };
            var room20 = new Room(6) { Coordinates = new Point(2, 0), Zone = zone };
            var room21 = new Room(7) { Coordinates = new Point(2, 1), Zone = zone };
            var room22 = new Room(8) { Coordinates = new Point(2, 2), Zone = zone };
            var room30 = new Room(9) { Coordinates = new Point(3, 0), Zone = zone };
            var room31 = new Room(10) { Coordinates = new Point(3, 1), Zone = zone };
            var room32 = new Room(11) { Coordinates = new Point(3, 2), Zone = zone };
            var room40 = new Room(12) { Coordinates = new Point(4, 0), Zone = zone };
            var room41 = new Room(13) { Coordinates = new Point(4, 1), Zone = zone };
            var room42 = new Room(14) { Coordinates = new Point(4, 2), Zone = zone };

            room01.Exits = new Dictionary<Direction, Room> {{Direction.South, room11}};
            room11.Exits = new Dictionary<Direction, Room> {{Direction.North, room01}, {Direction.South, room21}};
            room21.Exits = new Dictionary<Direction, Room> {{Direction.North, room11}, {Direction.South, room31}};
            room31.Exits = new Dictionary<Direction, Room> {{Direction.North, room21}, {Direction.South, room41}};
            room41.Exits = new Dictionary<Direction, Room> {{Direction.North, room31}};

            zone.Rooms.Add(room00);
            zone.Rooms.Add(room01);
            zone.Rooms.Add(room02);
            zone.Rooms.Add(room10);
            zone.Rooms.Add(room11);
            zone.Rooms.Add(room12);
            zone.Rooms.Add(room20);
            zone.Rooms.Add(room21);
            zone.Rooms.Add(room22);
            zone.Rooms.Add(room30);
            zone.Rooms.Add(room31);
            zone.Rooms.Add(room32);
            zone.Rooms.Add(room40);
            zone.Rooms.Add(room41);
            zone.Rooms.Add(room42);

            return zone;
        }

        private static Zone BuildCubeZone()
        {
            var zone = CreateEmptyZone(3, 3);

            var room00 = new Room(0) {Coordinates = new Point(0, 0), Zone = zone};
            var room01 = new Room(1) {Coordinates = new Point(0, 1), Zone = zone};
            var room02 = new Room(2) {Coordinates = new Point(0, 2), Zone = zone};
            var room10 = new Room(3) {Coordinates = new Point(1, 0), Zone = zone};
            var room11 = new Room(4) {Coordinates = new Point(1, 1), Zone = zone};
            var room12 = new Room(5) {Coordinates = new Point(1, 2), Zone = zone};
            var room20 = new Room(6) {Coordinates = new Point(2, 0), Zone = zone};
            var room21 = new Room(7) {Coordinates = new Point(2, 1), Zone = zone};
            var room22 = new Room(8) {Coordinates = new Point(2, 2), Zone = zone};

            room00.Exits = new Dictionary<Direction, Room> {{Direction.East, room01}, {Direction.South, room10}};
            room01.Exits = new Dictionary<Direction, Room> {{Direction.East, room02}, {Direction.South, room11}, {Direction.West, room00}};
            room02.Exits = new Dictionary<Direction, Room> {{Direction.South, room12}, {Direction.West, room01}};
            room10.Exits = new Dictionary<Direction, Room> {{Direction.North, room00}, {Direction.East, room11}, {Direction.South, room20}};
            room11.Exits = new Dictionary<Direction, Room>
                               {
                                   {Direction.North, room01},
                                   {Direction.East, room12},
                                   {Direction.South, room21},
                                   {Direction.West, room10}
                               };
            room12.Exits = new Dictionary<Direction, Room> {{Direction.North, room02}, {Direction.South, room22}, {Direction.West, room11}};
            room20.Exits = new Dictionary<Direction, Room> {{Direction.North, room10}, {Direction.East, room21}};
            room21.Exits = new Dictionary<Direction, Room> {{Direction.West, room20}, {Direction.North, room11}, {Direction.East, room22}};
            room22.Exits = new Dictionary<Direction, Room> {{Direction.West, room21}, {Direction.North, room12}};

            zone.Rooms.Add(room00);
            zone.Rooms.Add(room01);
            zone.Rooms.Add(room02);
            zone.Rooms.Add(room10);
            zone.Rooms.Add(room11);
            zone.Rooms.Add(room12);
            zone.Rooms.Add(room20);
            zone.Rooms.Add(room21);
            zone.Rooms.Add(room22);

            return zone;
        }

        private static Zone CreateEmptyZone(int mapWidth, int mapHeight)
        {
            return new Zone {Rooms = new List<Room>(mapWidth*mapHeight)};
        }

        public static void SpawnPlayerAtCenterOfMap(Player player, string mapType)
        {
            Room playerOrigin = null;

            switch (mapType.ToLower().Replace(" ", string.Empty))
            {
                case "cube":
                    playerOrigin = player.MoveTo(new Point(1, 1));
                    break;
                case "northtosouthhallway":
                    playerOrigin = player.MoveTo(new Point(2, 1));
                    break;
            }

            ScenarioContext.Current.Add("PlayerOrigin", playerOrigin);
        }
    }
}
