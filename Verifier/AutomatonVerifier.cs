using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verifier.Model;
using Verifier.Tla;
using Verifier.Xml;

namespace Verifier
{
    public class AutomatonVerifier
    {
        class PathItem
        {
            public ITlaState State { get; private set; }
            public ITlaTransition FromTransition { get; private set; }
            public PathItem Prev { get; private set; }

            public PathItem(ITlaState state, PathItem prev, ITlaTransition fromTransition)
            {
                this.State = state;
                this.Prev = prev;
                this.FromTransition = fromTransition;
            }

            public bool IsAny(ITlaState state)
            {
                for (var s = this; s != null; s = s.Prev)
                    if (s.State == state)
                        return true;

                return false;
            }
        }

        private readonly TlaAutomaton _modelAutomaton;

        public XmlGraph LastVerificationGraphInfo { get; private set; }

        public AutomatonVerifier(TlaAutomaton sm)
        {
            if (sm.AllTransitions.Any(t => !t.Condition.IsConjunction()))
                throw new ArgumentException();

            _modelAutomaton = sm;
        }

        public LinkedList<ITlaState> Verify(TlaAutomaton ltl)
        {
            var automaton = this.Intersect(_modelAutomaton, ltl);
            automaton.Optimize();
            var xg = automaton.ToXmlGraph();
            this.LastVerificationGraphInfo = xg;

            PathItem cycle = null;
            var path = automaton.InitialStates.Select(initState => this.RunAutomatonFrom(initState, step => {
                if (!step.State.IsAccepting)
                    return false;

                cycle = this.RunAutomatonFrom(step.State, cycleStep => step.IsAny(cycleStep.State));
                return cycle != null;
            })).FirstOrDefault(l => l != null);

            if (path != null)
            {
                for (var item = cycle; item != null; item = item.Prev)
                {
                    xg[item.State.Name].Background = "Blue";

                    if (item.FromTransition != null)
                        xg[item.FromTransition.FromState.Name].GetConnectionTargets().First(l => l.Target.Id == item.FromTransition.ToState.Name).Color = "Blue";
                }

                var list = new LinkedList<ITlaState>();
                for (var item = path; item != null; item = item.Prev)
                {
                    xg[item.State.Name].Background = "Green";
                    list.AddFirst(item.State);

                    if (item.FromTransition != null)
                        xg[item.FromTransition.FromState.Name].GetConnectionTargets().First(l => l.Target.Id == item.FromTransition.ToState.Name).Color = "Green";
                }

                return list;
            }
            else
            {
                return null;
            }
        }

        private PathItem RunAutomatonFrom(ITlaState initState, Func<PathItem, bool> finishCondition)
        {
            var handledStateIds = new SortedSet<int>();
            var stack = new Stack<PathItem>();
            stack.Push(new PathItem(initState, null, null));

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                var stateId = item.State.Id;

                if (!handledStateIds.Contains(stateId))
                {
                    handledStateIds.Add(stateId);

                    foreach (var transition in item.State.Outgoings)
                    {
                        var newItem = new PathItem(transition.ToState, item, transition);
                        if (finishCondition(newItem))
                            return newItem;

                        stack.Push(newItem);
                    }
                }
            }

            return null;
        }

        private TlaAutomaton Intersect(TlaAutomaton sm, TlaAutomaton ltl)
        {
            var states = Enumerable.Range(0, 3).SelectMany(n => sm.AllStates.SelectMany(modelState => ltl.AllStates.Select(ltlState => new {
                name = string.Format("{0}x{1}x{2}", modelState.Id, ltlState.Id, n),
                tag = modelState.Name
            }))).ToArray();

            var transitions = Enumerable.Range(0, 3).SelectMany(
                x => sm.AllTransitions.SelectMany(mt => ltl.AllTransitions.Select(ft => new { modelTransition = mt, ltlTransition = ft }))
                       .Where(tt => TransitionConditionsIntersects(tt.modelTransition.FromState, tt.modelTransition.Condition, tt.ltlTransition.Condition))
                       .Select(tt => new {
                           from = string.Format("{0}x{1}x{2}", tt.modelTransition.FromState.Id, tt.ltlTransition.FromState.Id, x),
                           to = string.Format("{0}x{1}x{2}", tt.modelTransition.ToState.Id, tt.ltlTransition.ToState.Id, ComputeTransitionY(tt.modelTransition.ToState, tt.ltlTransition.ToState, x)),
                           modelSymbol = tt.modelTransition.Condition,
                           ltlSymbol = tt.ltlTransition.Condition,
                           condition = new TlaTransitionConditionFormula(new TransitionConditionExpr.BinaryExpr(
                               TransitionConditionBinaryExprKind.BoolAnd,
                               new TransitionConditionExpr.VarExpr(tt.modelTransition.FromState.Name),
                               (tt.modelTransition.Condition as TlaTransitionConditionFormula).Expression
                           ))
                       })
            ).ToArray();

            var initialStates = new SortedSet<string>(sm.InitialStates.SelectMany(modelState => ltl.InitialStates.Select(ltlState => string.Format("{0}x{1}x0", modelState.Id, ltlState.Id))));
            var acceptingStates = new SortedSet<string>(sm.AllStates.SelectMany(modelState => ltl.AllStates.Select(ltlState => string.Format("{0}x{1}x2", modelState.Id, ltlState.Id))));

            var result = new TlaAutomaton();

            foreach (var state in states)
                result.CreateState(state.name, initialStates.Contains(state.name), acceptingStates.Contains(state.name)).Tag = state.tag;

            foreach (var t in transitions)
                result.CreateTransition(t.from, t.to, t.condition);

            return result;
        }

        private int ComputeTransitionY(ITlaState modelTargetState, ITlaState ltlTargetState, int x)
        {
            int y;

            if (x == 0 && modelTargetState.IsAccepting)
                y = 1;
            else if (x == 1 && ltlTargetState.IsAccepting)
                y = 2;
            else if (x == 2)
                y = 0;
            else
                y = x;

            return y;
        }

        private bool TransitionConditionsIntersects(ITlaState modelState, TlaFormula modelCondition, TlaFormula ltlFormula)
        {
            var varNames = modelCondition.GetAllVars();

            var result = ltlFormula.Evaluate(vname => vname == modelState.Name || modelState.Name.StartsWith(vname + "|") || varNames.Contains(vname));

            return result;
        }

    }
}
