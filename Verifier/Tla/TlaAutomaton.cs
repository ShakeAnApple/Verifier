using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Verifier.Tla
{
    public abstract class TlaFormula
    {
        public bool IsConjunction()
        {
            return this.IsConjunctionImpl();
        }

        protected abstract bool IsConjunctionImpl();

        public SortedSet<string> GetAllVars()
        {
            return this.GetAllVarsImpl();
        }

        protected abstract SortedSet<string> GetAllVarsImpl();

        public bool Evaluate(Func<string, bool> varExpr)
        {
            return this.EvaluateImpl(varExpr);
        }

        protected abstract bool EvaluateImpl(Func<string, bool> varExpr);
    }

    public interface ITlaState
    {
        // TlaAutomaton Automaton { get; }

        int Id { get; }
        string Name { get; }
        bool IsInitial { get; }
        bool IsAccepting { get; }

        ReadOnlyCollection<ITlaTransition> Outgoings { get; }
        ReadOnlyCollection<ITlaTransition> Incomings { get; }

        object Tag { get; set; }
    }

    public interface ITlaTransition
    {
        int Id { get; }

        ITlaState FromState { get; }
        ITlaState ToState { get; }

        TlaFormula Condition { get; }
    }

    public class TlaAutomaton
    {
        class TlaState : ITlaState
        {
            public TlaAutomaton Automaton { get { return _owner; } }

            public int Id { get; private set; }
            public string Name { get; private set; }
            public bool IsInitial { get; private set; }
            public bool IsAccepting { get; private set; }

            readonly TlaAutomaton _owner;
            readonly List<TlaTransition> _outTransitions = new List<TlaTransition>();
            readonly List<TlaTransition> _inTransitions = new List<TlaTransition>();

            public ReadOnlyCollection<TlaTransition> Outgoings { get { return new ReadOnlyCollection<TlaTransition>(_outTransitions.ToArray()); } }
            public ReadOnlyCollection<TlaTransition> Incomings { get { return new ReadOnlyCollection<TlaTransition>(_inTransitions.ToArray()); } }

            ReadOnlyCollection<ITlaTransition> ITlaState.Outgoings { get { return new ReadOnlyCollection<ITlaTransition>(_outTransitions.ToArray()); } }
            ReadOnlyCollection<ITlaTransition> ITlaState.Incomings { get { return new ReadOnlyCollection<ITlaTransition>(_inTransitions.ToArray()); } }

            public object Tag { get; set; }

            public TlaState(TlaAutomaton owner, int id, string name, bool isInitial, bool isAccepting)
            {
                _owner = owner;

                this.Id = id;
                this.Name = name; //.LowerFirstCharacter();
                this.IsInitial = isInitial;
                this.IsAccepting = isAccepting;
            }

            public void RegisterOutgoing(TlaTransition transition) { _outTransitions.Add(transition); }
            public void RegisterIncoming(TlaTransition transition) { _inTransitions.Add(transition); }
            public void UnregisterIncoming(TlaTransition t) { _inTransitions.Remove(t); }

            public override string ToString()
            {
                return string.Format("S#{0}:{1}", this.Id, this.Name);
            }
        }

        class TlaTransition : ITlaTransition
        {
            public int Id { get; private set; }

            public ITlaState FromState { get; private set; }
            public ITlaState ToState { get; private set; }

            public TlaFormula Condition { get; private set; }

            public TlaTransition(int id, TlaState from, TlaState to, TlaFormula condition)
            {
                this.Id = id;
                this.FromState = from;
                this.ToState = to;
                this.Condition = condition;

                to.RegisterIncoming(this);
                from.RegisterOutgoing(this);
            }

            public override string ToString()
            {
                return string.Format("T#{0}: {1} --> {2} when {3}", this.Id, this.FromState, this.ToState, this.Condition);
            }
        }

        readonly Dictionary<string, TlaState> _statesByName = new Dictionary<string, TlaState>(); // StringComparer.InvariantCultureIgnoreCase);
        readonly Dictionary<int, TlaState> _statesById = new Dictionary<int, TlaState>();

        readonly List<TlaTransition> _allTransitions = new List<TlaTransition>();

        readonly List<TlaState> _initialStates = new List<TlaState>();
        readonly List<TlaState> _acceptingStates = new List<TlaState>();

        public ReadOnlyCollection<ITlaState> AllStates { get { return new ReadOnlyCollection<ITlaState>(_statesById.Values.ToArray()); } }
        public ReadOnlyCollection<ITlaTransition> AllTransitions { get { return new ReadOnlyCollection<ITlaTransition>(_allTransitions.ToArray()); } }

        public ReadOnlyCollection<ITlaState> InitialStates { get { return new ReadOnlyCollection<ITlaState>(_initialStates.ToArray()); } }
        public ReadOnlyCollection<ITlaState> AcceptingStates { get { return new ReadOnlyCollection<ITlaState>(_acceptingStates.ToArray()); } }

        public TlaAutomaton()
        {
        }

        public ITlaState CreateState(string name, bool isInitial, bool isAccepting)
        {
            var state = new TlaState(this, _statesById.Count, name, isInitial, isAccepting);
            _statesById.Add(state.Id, state);
            _statesByName.Add(state.Name, state);

            if (isInitial)
                _initialStates.Add(state);

            if (isAccepting)
                _acceptingStates.Add(state);

            return state;
        }

        public ITlaTransition CreateTransition(int idFrom, int idTo, TlaFormula condition)
        {
            var transition = new TlaTransition(_allTransitions.Count, _statesById[idFrom], _statesById[idTo], condition);
            _allTransitions.Add(transition);
            return transition;
        }

        public ITlaTransition CreateTransition(string nameFrom, string nameTo, TlaFormula condition)
        {
            var transition = new TlaTransition(_allTransitions.Count, _statesByName[nameFrom], _statesByName[nameTo], condition);
            _allTransitions.Add(transition);
            return transition;
        }

        public ITlaState GetState(string name)
        {
            return _statesByName[name];
        }

        public ITlaState GetState(int id)
        {
            return _statesById[id];
        }

        public override string ToString()
        {
            return string.Format("{{{0}}} --> ... --> {{{1}}}", string.Join(", ", _initialStates), string.Join(", ", _acceptingStates));
        }

        /// <summary>
        /// remove all states without incoming transitions except initial
        /// </summary>
        public void Optimize()
        {
            while (this.TryOptimize()) ;
        }

        private bool TryOptimize()
        {
            var states = _statesById.Values.ToArray();

            foreach (var state in states)
            {
                if (!state.IsInitial && state.Incomings.Count == 0)
                    this.RemoveState(state);
            }

            return states.Length != _statesById.Count;
        }

        private void RemoveState(TlaState state)
        {
            _statesById.Remove(state.Id);
            _statesByName.Remove(state.Name);

            foreach (var t in state.Outgoings)
            {
                _statesById[t.ToState.Id].UnregisterIncoming(t);
                _allTransitions.Remove(t);
            }
        }
    }
}
