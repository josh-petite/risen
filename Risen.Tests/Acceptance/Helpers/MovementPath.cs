using System.Collections.Generic;

namespace Risen.Tests.Acceptance.Helpers
{
    public class MovementPath
    {
        public MovementPath()
        {
            Steps = new List<KeyValuePair<string, int>>();
        }

        public List<KeyValuePair<string, int>> Steps { get; set; }
    }
}