using Risen.Tests.Acceptance.Helpers;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Steps
{
    [Binding]
    public class MapSteps
    {
        [Given(@"I have a (.*) map")]
        public void GivenIHaveACrossShapedMap(string mapType)
        {
            ScenarioContext.Current.Add(string.Format("{0} map", mapType), MapHelper.BuildMapFromMapType(mapType));
        }

        [Given(@"the player is in a north to south hallway")]
        public void GivenThePlayerIsInANorthToSouthHallway()
        {
            var mapType = "North to South Hallway";
            ScenarioContext.Current.Add(mapType, MapHelper.BuildMapFromMapType(mapType));
        }

    }
}
