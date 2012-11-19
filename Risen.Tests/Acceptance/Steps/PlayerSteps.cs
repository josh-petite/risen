using NUnit.Framework;
using Risen.Logic.Entities;
using Risen.Tests.Acceptance.Helpers;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Steps
{
    [Binding]
    public class PlayerSteps
    {
        [Given(@"I have a character")]
        public void GivenIHaveACharacter()
        {
            ScenarioContext.Current.Add("Player", new Player());
        }

        [Given(@"my player is at the center of the (.*) map")]
        public void GivenMyPlayerIsAtTheCenterOfAMap(string mapType)
        {
            var player = PlayerHelper.GetPlayerFromContext();
            player.Zone = MapHelper.GetMapFromContext();

            MapHelper.SpawnPlayerAtCenterOfMap(player, mapType);
        }
        
        [When(@"the player moves (north|south|east|west|up|down) (.*) rooms?")]
        public void WhenThePlayerMoves(string direction, int numberOfRooms)
        {
            PlayerHelper.MovePlayer(direction, numberOfRooms);
        }

        [Then(@"the player should be (.*) of its origin")]
        public void ThenThePlayerShouldBeAtExpectedLocationComparedToItsOrigin(string expectedMovement)
        {
            var player = PlayerHelper.GetPlayerFromContext();
            var movementPath = PlayerHelper.ParseMovement(expectedMovement);
            var destinationLocation = PlayerHelper.GetDestinationLocation(player, movementPath);

            Assert.AreEqual(player.Location, destinationLocation);
        }

        [Then(@"the player should be at its origin")]
        public void ThenThePlayerShouldBeAtItsOrigin()
        {
            var player = PlayerHelper.GetPlayerFromContext();
            Assert.AreEqual(player.Location, player.Origin);
        }
    }
}