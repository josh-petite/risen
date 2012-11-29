using System;
using System.Collections.Generic;
using System.Linq;
using Risen.Server.Entities;
using Risen.Server.Enums;
using Risen.Server.ReferenceTypes;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Helpers
{
    public static class PlayerHelper
    {
        public static Player GetPlayerFromContext()
        {
            return (Player)ScenarioContext.Current.Single(o => o.Value is Player).Value;
        }

        public static Room GetPlayerOriginFromContext()
        {
            return (Room) ScenarioContext.Current["PlayerOrigin"];
        }

        public static void MovePlayer(Exit exit, int numberOfRooms)
        {
            var player = GetPlayerFromContext();

            for (int i = 0; i < numberOfRooms; i++)
                player.MoveTo(exit);
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
            var location = new Room {Coordinates = playerOrigin.Coordinates, RoomExits = playerOrigin.RoomExits};

            foreach (var step in movementPath.Steps)
            {
                switch (step.Key)
                {
                    case "N":
                        location = location.GetRoomInDirectionOf(ExitTemplateRef.North);
                        break;
                    case "S":
                        location = location.GetRoomInDirectionOf(ExitTemplateRef.South);
                        break;
                    case "E":
                        location = location.GetRoomInDirectionOf(ExitTemplateRef.East);
                        break;
                    case "W":
                        location = location.GetRoomInDirectionOf(ExitTemplateRef.West);
                        break;
                }
            }

            return location;
        }
    }
}