using System;
using System.Collections.Generic;
using System.Linq;
using Risen.Server.Entities;
using Risen.Server.Enums;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Helpers
{
    public static class PlayerHelper
    {
        public static Player GetPlayerFromContext()
        {
            return (Player)ScenarioContext.Current.Single(o => o.Value.GetType() == typeof(Player)).Value;
        }

        public static Room GetPlayerOriginFromContext()
        {
            return (Room) ScenarioContext.Current["PlayerOrigin"];
        }

        public static void MovePlayer(Direction direction, int numberOfRooms)
        {
            var player = GetPlayerFromContext();

            for (int i = 0; i < numberOfRooms; i++)
                player.MoveTo(direction);
        }

        public static MovementPath ParseMovement(string expectedMovement)
        {
            var steps = expectedMovement.Split(new[] {'|'}, 2);
            var path = new MovementPath();

            foreach (var step in steps)
            {
                var direction = step.Substring(0, 1);
                var distance = Convert.ToInt32(step.Substring(1, 1));
                path.Steps.Add(new KeyValuePair<string, int>(direction, distance));
            }

            return path;
        }

        public static Room GetDestinationLocation(Player player, MovementPath movementPath)
        {
            var playerOrigin = GetPlayerOriginFromContext();
            var location = new Room {Coordinates = playerOrigin.Coordinates, Exits = playerOrigin.Exits};

            foreach (var step in movementPath.Steps)
            {
                switch (step.Key)
                {
                    case "N":
                        location = location.Exits[Direction.North];
                        break;
                    case "S":
                        location = location.Exits[Direction.South];
                        break;
                    case "E":
                        location = location.Exits[Direction.East];
                        break;
                    case "W":
                        location = location.Exits[Direction.West];
                        break;
                }
            }

            return location;
        }
    }
}