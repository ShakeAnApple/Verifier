using System.Collections.Generic;

namespace Verifier.Model
{
    public class State
    {
        public int Id { get; private set; }

        public State(int id)
        {
            this.Id = id;
        }

        public string Name { get; set; }

        public List<Transition> Outgoing { get; set; }
        public List<Transition> Incoming { get; set; }

        public bool IsInitial { get; set; }
        public bool IsAccepting { get; set; }
    }
}
