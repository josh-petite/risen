using System.Collections.Generic;

namespace Risen.Server.Entities
{
    public class NonPlayerCharacter : MobileEntity
    {
        public IEnumerable<Comment> Comments { get; set; }
    }
}