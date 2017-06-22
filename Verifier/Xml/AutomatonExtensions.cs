using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verifier.Tla;

namespace Verifier.Xml
{
    static class AutomatonExtensions
    {
        public static void SaveAsDgmlGraph(this TlaAutomaton automaton, string fileName)
        {
            automaton.ToXmlGraph().MakeXmlDocument().Save(fileName);
        }

        public static XmlGraph ToXmlGraph(this TlaAutomaton automaton)
        {
            var xg = new XmlGraph();

            foreach (var state in automaton.AllStates)
            {
                string name = string.Empty;

                if (state.IsInitial)
                    name += "Initial" + Environment.NewLine;

                name += state.Name;

                if (state.IsAccepting)
                    name += Environment.NewLine + "Accepting";

                xg.CreateNode(state.Name).Text = name;
            }

            foreach (var item in automaton.AllTransitions)
            {
                xg[item.FromState.Name].ConnectTo(xg[item.ToState.Name]).Text = item.Condition == null ? "<NULL>" : item.Condition.ToString();
            }

            return xg;
        }
    }
}
