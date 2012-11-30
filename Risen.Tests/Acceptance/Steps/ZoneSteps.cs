using Risen.Tests.Acceptance.Helpers;
using TechTalk.SpecFlow;

namespace Risen.Tests.Acceptance.Steps
{
    [Binding]
    public class ZoneSteps
    {
        [Given(@"I have a (.*) zone")]
        public void GivenIHaveAZone(string mapType)
        {
            var zone = ZoneHelper.BuildMapFromMapType(mapType);
            ScenarioContext.Current.Add(string.Format("{0} map", mapType), zone);
        }

        [Given(@"the player is in a north to south hallway")]
        public void GivenThePlayerIsInANorthToSouthHallway()
        {
            const string mapType = "North to South Hallway";
            ScenarioContext.Current.Add(string.Format("{0} map", mapType), ZoneHelper.BuildMapFromMapType(mapType));
        }
    }
}
