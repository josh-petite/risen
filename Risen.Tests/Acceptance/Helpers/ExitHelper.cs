using Risen.Server.Data;
using Risen.Server.Entities;
using Risen.Server.ReferenceTypes;

namespace Risen.Tests.Acceptance.Helpers
{
    public static class ExitHelper
    {
        public static Exit GetExitByName(string exitName)
        {
            var repository = new Repository();
            var exitId = (int) typeof (ExitTemplate).GetField(exitName).GetValue(null);
            return repository.FindOne<Exit>(o => o.ExitTemplate.Id == exitId);
        }
    }
}
