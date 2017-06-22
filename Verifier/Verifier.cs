using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verifier.Model;

namespace Verifier
{
    public class Verifier
    {
        private readonly Automaton _sm;

        public Verifier(Automaton sm)
        {
            _sm = sm;
        }

        public LinkedList<State> Verify(Automaton ltlProp)
        {
            // cross automatas
            throw new NotImplementedException();
        }
    }
}
