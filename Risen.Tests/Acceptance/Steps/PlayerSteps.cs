using System;
using NUnit.Framework;
using Risen.Server.Entities;
using Risen.Tests.Acceptance.Helpers;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Steps
{
    [Binding]
    public class PlayerSteps
    {
        [Given(@"I have a player")]
        public void GivenIHaveAPlayer()
        {
            ScenarioContext.Current.Add("Player", new Player());
        }

        [Given(@"my player is at the center of the (.*) zone")]
        public void GivenMyPlayerIsAtTheCenterOfAZone(string mapType)
        {
            var player = PlayerHelper.GetPlayerFromContext();
            player.CurrentRoom = ZoneHelper.GetZoneFromContext().GetSpawnRoom();
            ZoneHelper.SpawnPlayerAtCenterOfMap(player, mapType);
        }
        
        [When(@"the player moves (North|South|East|West|Northwest|Northeast|Southwest|Southeast|Up|Down) (.*) rooms?")]
        public void WhenThePlayerMoves(string exit, int numberOfRooms)
        {
            PlayerHelper.MovePlayer(ExitHelper.GetExitByName(exit), numberOfRooms);
        }

        [Then(@"the player should be (.*) of its origin")]
        public void ThenThePlayerShouldBeAtExpectedLocationComparedToItsOrigin(string expectedMovement)
        {
            var player = PlayerHelper.GetPlayerFromContext();
            var movementPath = PlayerHelper.ParseMovement(expectedMovement);
            var destinationLocation = PlayerHelper.GetDestinationLocation(player, movementPath);

            Assert.AreEqual(player.CurrentRoom, destinationLocation);
        }

        [Then(@"the player should be at its origin")]
        public void ThenThePlayerShouldBeAtItsOrigin()
        {
            var player = PlayerHelper.GetPlayerFromContext();
            Assert.AreEqual(player.CurrentRoom, PlayerHelper.GetPlayerOriginFromContext());
        }
    }
}