using System.Collections.Generic;

namespace Risen.Server.Entities
{
    public class Party
    {
        public Player Leader { get; set; }
        public List<Player> Members { get; set; }

        public void DistributeExperienceGainedToParty(int experienceGained)
        {
            Members.ForEach(o => o.Experience += experienceGained);
        }
    }
}
