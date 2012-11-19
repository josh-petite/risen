using System.Collections.Generic;

namespace Risen.Logic.Entities
{
    public class ZoneCache
    {
        static ZoneCache()
        {
            Cache = new List<Zone>();
        }

        public static List<Zone> Cache { get; set; }

        public void LoadZoneIntoCache(uint zoneId)
        {
            Cache.Add(new Zone()); // call fluent for value in the future
        }
    }
}