using System;
using System.Collections.Generic;
using System.Linq;
using Risen.Logic.Entities;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Helpers
{
    public static class PlayerHelper
    {
        public static Player GetPlayerFromContext()
        {
            return (Player)ScenarioContext.Current.Single(o => o.Value.GetType() == typeof(Player)).Value;
        }

        public static void MovePlayer(string direction, int numberOfRooms)
        {
            var player = GetPlayerFromContext();

            for (int i = 0; i < numberOfRooms; i++)
                player.Move(direction);
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
            var location = new Room(player.Origin);

            foreach (var step in movementPath.Steps)
            {
                switch (step.Key)
                {
                    case "N":
                        location = location.Exits.North;
                        break;
                    case "S":
                        location = location.Exits.South;
                        break;
                    case "E":
                        location = location.Exits.East;
                        break;
                    case "W":
                        location = location.Exits.West;
                        break;
                }
            }

            return location;
        }
    }
}