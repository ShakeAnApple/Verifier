using System.Collections.Generic;

namespace Verifier.Model
{
   public class Transition
    {
        public int Id { get; private set; }

        public Transition(int id)
        {
            this.Id = id;
        }

        public string EventName { get; set; }
        public List<string> Actions { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }

        public TransitionConditionExpr Condition { get; set; }
    }
}
