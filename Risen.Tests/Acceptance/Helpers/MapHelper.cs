using System.Linq;
using Risen.Logic.Entities;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Helpers
{
    public static class MapHelper
    {
        public static Zone GetMapFromContext()
        {
            return (Zone)ScenarioContext.Current.Single(o => o.Value.GetType() == typeof(Zone)).Value;
        }

        public static Zone BuildMapFromMapType(string mapType)
        {
            switch(mapType.ToLower())
            {
                case "cube":
                    return BuildCubeMap();
                case "north to south hallway":
                    return NorthToSouthHallway();
                default:
                    return new Zone();
            }
        }

        private static Zone NorthToSouthHallway()
        {
            var map = CreateEmptyMap(3, 10);

            for (int i = 0; i < map.Rooms.GetUpperBound(1); i++)
            {
                map.Rooms[1, i].Exits = new Exits {North = new Room(), South = new Room()};
            }

            return map;
        }

        private static Zone BuildCubeMap()
        {
            var map = CreateEmptyMap(3,3);

            map.Rooms[0, 0].Exits = new Exits { East = new Room(), South = new Room() };
            map.Rooms[0, 1].Exits = new Exits { West = new Room(), South = new Room(), East = new Room() };
            map.Rooms[0, 2].Exits = new Exits { West = new Room(), South = new Room() };
            map.Rooms[1, 0].Exits = new Exits { North = new Room(), East = new Room(), South = new Room() };
            map.Rooms[1, 1].Exits = new Exits { North = new Room(), South = new Room(), East = new Room(), West = new Room() };
            map.Rooms[1, 2].Exits = new Exits { North = new Room(), South = new Room(), West = new Room() };
            map.Rooms[2, 0].Exits = new Exits { North = new Room(), East = new Room() };
            map.Rooms[2, 1].Exits = new Exits { West = new Room(), North = new Room(), East = new Room() };
            map.Rooms[2, 2].Exits = new Exits { North = new Room(), West = new Room() };

            return map;
        }

        private static Zone CreateEmptyMap(int mapWidth, int mapHeight)
        {
            var map = new Zone {Rooms = new Room[mapWidth,mapHeight]};

            for (int i = 0; i < map.Rooms.GetLength(0); i++)
                for (int j = 0; j < map.Rooms.GetLength(1); j++)
                    map.Rooms[i, j] = new Room();

            return map;
        }

        public static void SpawnPlayerAtCenterOfMap(Player player, string mapType)
        {
            switch (mapType.ToLower())
            {
                case "cube":
                    player.SetOrigin(1, 1);
                    player.SetCurrentRoom(1, 1);
                    break;
                case "northtosouthhallway":
                    player.SetOrigin(1,5);
                    player.SetCurrentRoom(1, 5);
                    break;
            }
        }
    }
}
