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

        class ParallelPathItem : PathItem
        {
            public new ParallelPathItem Prev { get; private set; }

            public ITlaState CheckerState { get; private set; }
            public ITlaTransition CheckerFromTransition { get; private set; }

            public int Length { get; private set; }

            public ParallelPathItem(ParallelPathItem prev, ITlaState state, ITlaTransition fromTransition, ITlaState checkerState, ITlaTransition checkerFromTransition)
                : base(state, prev, fromTransition)
            {
                this.Prev = prev;
                this.CheckerState = checkerState;
                this.CheckerFromTransition = checkerFromTransition;
                this.Length = prev == null ? 1 : prev.Length;
            }

            public bool IsCheckersAny(ITlaState checkerState)
            {
                for (var s = this; s != null; s = s.Prev)
                    if (s.CheckerState == checkerState)
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

            // loops on original model?
            this.AddLoopStateToFinalStates(sm);

            _modelAutomaton = sm;
        }

        private void AddLoopStateToFinalStates(TlaAutomaton a)
        {
            foreach (var st in a.AcceptingStates)
                if (!st.Name.Contains("|") && st.Outgoings.Count == 0)
                    a.CreateTransition(st.Id, st.Id, new TlaTransitionConditionFormula(new TransitionConditionExpr.ConstExpr(true)));
        }

        public LinkedList<ITlaState> Verify(TlaAutomaton ltl, bool intersect = true)
        {
            // this.AddLoopStateToFinalStates(ltl);

            return intersect ? VerifyWithIntersection(ltl) : VerifyWithTraverse(ltl);
        }

        private LinkedList<ITlaState> VerifyWithTraverse(TlaAutomaton ltl)
        {
            var limit = _modelAutomaton.AllStates.Count * ltl.AllStates.Count;

            PathItem statement = null;
            var path = _modelAutomaton.InitialStates.Select(initState => this.RunAutomatonTraversingFrom(initState, step => {
                if (!step.State.Name.Contains('|'))
                {
                    statement = ltl.InitialStates.Select(checkerInitState => this.RunChecker(step.State, checkerInitState, limit)).FirstOrDefault(p => p != null);
                    return statement != null;
                }
                return false;
            })).FirstOrDefault(p => p != null);

            return this.MakeResult(_modelAutomaton, path, statement);
        }

        private PathItem RunAutomatonTraversingFrom(ITlaState initState, Func<PathItem, bool> finishCondition)
        {
            var handledStateIds = new SortedSet<int>();
            var stack = new Stack<PathItem>();
            stack.Push(new PathItem(initState, null, null));

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                var stateId = item.State.Id;

                if (finishCondition(item))
                    return item;

                if (!handledStateIds.Contains(stateId))
                {
                    handledStateIds.Add(stateId);

                    foreach (var transition in item.State.Outgoings)
                        stack.Push(new PathItem(transition.ToState, item, transition));
                }
            }

            return null;
        }

        private PathItem RunChecker(ITlaState from, ITlaState checker, int limit)
        {
            var stack = new Stack<ParallelPathItem>();
            stack.Push(new ParallelPathItem(null, from, null, checker, null));
            Console.WriteLine("Running checker from {0}", from.Name);

            while (stack.Count > 0)
            {
                var item = stack.Pop();
                if (item.CheckerState.IsAccepting && item.State.IsAccepting)
                    return item;

                foreach (var modelTransition in item.State.Outgoings)
                {
                    foreach (var checkerTransition in item.CheckerState.Outgoings)
                    {
                        if (this.TransitionConditionsIntersects(item.State, modelTransition.Condition, checkerTransition.Condition))
                        {
                            if (!item.IsAny(modelTransition.ToState) && !item.IsCheckersAny(checkerTransition.ToState))
                            {
                                var newItem = new ParallelPathItem(item, modelTransition.ToState, modelTransition, checkerTransition.ToState, checkerTransition);
                                Console.WriteLine("\t{0}({1}) -> {2}({3})", item.State, item.CheckerState, newItem.State, newItem.CheckerState);

                                if (newItem.Length % limit == 0)
                                {
                                    Console.WriteLine("Traverse depth warning: {0}! Press any key to continue, B to break particular path, A to abort traversing.", newItem.Length);

                                    switch (Console.ReadKey().Key)
                                    {
                                        case ConsoleKey.B: continue;
                                        case ConsoleKey.A: return null;
                                    }
                                }

                                stack.Push(newItem);
                            }
                        }
                    }
                }
            }

            return null;
        }

        private LinkedList<ITlaState> VerifyWithIntersection(TlaAutomaton ltl)
        {
            var automaton = this.Intersect(_modelAutomaton, ltl);
            automaton.Optimize();

            PathItem cycle = null;
            var path = automaton.InitialStates.Select(initState => this.RunAutomatonFrom(initState, step => {
                //if (step.Prev.IsAny(step.State))
                //    return true;

                if (!step.State.IsAccepting)
                    return false;

                cycle = this.RunAutomatonFrom(step.State, cycleStep => step.IsAny(cycleStep.State));
                return cycle != null;
            })).FirstOrDefault(l => l != null);

            //var r = new DoubeDfs(automaton).emptiness();
            //Console.WriteLine("DFS: {0}", r ? "found path" : "no path");

            //if ((path != null) == r)
            //    Console.WriteLine("OK");
            //else
            //    Console.WriteLine("FAIL");

            return this.MakeResult(automaton, path, cycle);
        }

        LinkedList<ITlaState> MakeResult(TlaAutomaton automaton, PathItem path, PathItem cycle)
        {
            var xg = automaton.ToXmlGraph();
            this.LastVerificationGraphInfo = xg;

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

        class DoubeDfs
        {
            readonly TlaAutomaton _automaton;
            readonly ITlaState _initState;

            readonly SortedSet<int> _hash = new SortedSet<int>();
            readonly SortedSet<int> _flag = new SortedSet<int>();

            public DoubeDfs(TlaAutomaton automaton)
            {
                _automaton = automaton;
            }

            public bool emptiness()
            {
                //for all q0 € Q0 do
                //        dfsl(q0);

                foreach (var q0 in _automaton.InitialStates)
                    if (dfs1(q0))
                        return true;

                //terminate(False);
                return false;
            }

            bool dfs1(ITlaState q)
            {
                _hash.Add(q.Id);

                //for all последователей q' вершины q do
                //    if q' не содержится в хэш-таблице then dfsl (q');

                foreach (var q1 in q.Outgoings.Select(t => t.ToState))
                    if (!_hash.Contains(q1.Id))
                        if (dfs1(q1))
                            return true;

                //if accept(q) then dfs2(q);
                if (q.IsAccepting)
                    if (dfs2(q))
                        return true;

                return false;
            }

            bool dfs2(ITlaState q)
            {
                _flag.Add(q.Id);

                //for all последователей q' вершины q do
                //    if q' в стеке dfsl then terminate(True);
                //    else if q' не является помеченной then dfs2(q')',
                //    end if;

                foreach (var q1 in q.Outgoings.Select(t => t.ToState))
                    if (_hash.Contains(q1.Id))
                        return true;
                    else if (!_flag.Contains(q1.Id))
                        dfs2(q1);

                return false;
            }
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

            // var result = ltlFormula.Evaluate(vname => varNames.Contains(vname));
            var result = ltlFormula.Evaluate(vname => vname == modelState.Name || modelState.Name.StartsWith(vname + "|") || varNames.Contains(vname));

            return result;
        }

    }
}
