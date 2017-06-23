using System.Collections.Generic;
using System.Linq;
using Verifier.Model;

namespace Verifier.Xml
{
    partial class XmlDiagramType
    {
        public Automaton ToAutomata()
        {
            var automaton = new Automaton {
                States = new List<State>()
            };

            var transitions = this.widget.Where(w => w.type == WidgetType.Transition).ToList();
            var states = this.widget.Where(w => w.type == WidgetType.State).ToList();

            foreach (var st in states)
            {
                var incomingIds = st.attributes.incoming != null ? st.attributes.incoming.Select(t => t.id).ToList() : new List<int>();
                var outgoingIds = st.attributes.outgoing != null ? st.attributes.outgoing.Select(t => t.id).ToList() : new List<int>();

                var incoming = transitions.Where(t => incomingIds.Contains(t.id))
                    .Select(t => new Transition(t.id) {
                        Actions = t.attributes.action != null ?
                                                    t.attributes.action.Select(a => a.name).ToList()
                                                    : new List<string>(),
                        EventName = t.attributes.@event.name,
                        FromId = (states.FirstOrDefault(
                            innerS => innerS.attributes.outgoing == null ? false : innerS.attributes.outgoing.Select(innerT => innerT.id).Contains(t.id)
                        ) ?? new XmlWidgetType { id = st.id }).id,
                        ToId = st.id
                    }).ToList();

                var outgoing = transitions.Where(t => outgoingIds.Contains(t.id))
                    .Select(t => new Transition(t.id) {
                        Actions = t.attributes.action != null ?
                                                    t.attributes.action.Select(a => a.name).ToList()
                                                    : new List<string>(),
                        EventName = t.attributes.@event.name,
                        FromId = st.id,
                        ToId = (states.FirstOrDefault(
                            innerS => innerS.attributes.incoming == null ? false : innerS.attributes.incoming.Select(innerT => innerT.id).Contains(t.id)
                        ) ?? new XmlWidgetType { id = st.id }).id
                    }).ToList();

                var state = new State(st.id) {
                    Name = st.attributes.name,
                    Incoming = incoming,
                    Outgoing = outgoing,
                    IsInitial = st.attributes.type == 1,
                    IsAccepting = true
                };

                automaton.States.Add(state);
            }

            return automaton;
        }
    }
}
