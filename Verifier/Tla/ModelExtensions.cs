using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verifier.Model;

namespace Verifier.Tla
{
    static class ModelExtensions
    {
        //public static string LowerFirstCharacter(this string str)
        //{
        //    if (string.IsNullOrWhiteSpace(str))
        //        return str;

        //    return char.ToLower(str[0]) + str.Substring(1);
        //}

        public static TlaAutomaton ToTlaAutomaton(this Automaton automaton, bool useTransitionConditions)
        {
            var result = new TlaAutomaton();

            foreach (var state in automaton.States)
                result.CreateState(state.Name, state.IsInitial, state.IsAccepting);

            foreach (var state in automaton.States)
            {
                foreach (var transition in state.Outgoing)
                {
                    if (transition.Actions == null || transition.Actions.Count == 0)
                    {
                        result.CreateTransition(
                            result.GetState(automaton.States.First(s => s.Id == transition.FromId).Name).Id,
                            result.GetState(automaton.States.First(s => s.Id == transition.ToId).Name).Id,
                            transition.MakeConditionFormula(useTransitionConditions)
                        );
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(transition.EventName))
                            throw new NotImplementedException("");

                        var statesSeq = transition.Actions.Select(actionName => result.CreateState($"{state.Name}|{transition.EventName}_before_{actionName}", false, false))
                                                  .Concat(new[] { result.GetState(automaton.States.First(s => s.Id == transition.ToId).Name) })
                                                  .ToArray();

                        result.CreateTransition(
                            result.GetState(automaton.States.First(s => s.Id == transition.FromId).Name).Id,
                            statesSeq.First().Id,
                            new TlaTransitionConditionFormula(new TransitionConditionExpr.VarExpr(transition.EventName))
                        );

                        for (int i = 0; i < transition.Actions.Count; i++)
                        {
                            var actionName = transition.Actions[i];
                            var condition = new TlaTransitionConditionFormula(new TransitionConditionExpr.VarExpr(actionName));
                            result.CreateTransition(statesSeq[i].Id, statesSeq[i + 1].Id, condition);
                        }
                    }
                }
            }

            return result;
        }

        static TlaFormula MakeConditionFormula(this Transition transition, bool useTransitionConditions)
        {
            if (transition.Condition != null && (transition.Actions != null || !string.IsNullOrWhiteSpace(transition.EventName) || transition.Actions.Any()))
                throw new ArgumentException("expecting only one form of condition on transition");
            if (!string.IsNullOrWhiteSpace(transition.EventName) && transition.Actions.Any())
                throw new ArgumentException("too complex transition, needs decomposition");

            if (useTransitionConditions)
                return new TlaTransitionConditionFormula(transition.MakeTransitionConditionExpr());
            else
                return new TlaExprFormula(transition.MakeTlaExpr());
        }

        static TransitionConditionExpr MakeTransitionConditionExpr(this Transition transition)
        {
            if (transition.Condition != null)
                return transition.Condition;

            if (!string.IsNullOrWhiteSpace(transition.EventName))
                return new TransitionConditionExpr.VarExpr(transition.EventName);

            if (transition.Actions.Any())
                return transition.Actions.Select(a => new TransitionConditionExpr.VarExpr(a))
                                 .Aggregate<TransitionConditionExpr>((l, r) => new TransitionConditionExpr.BinaryExpr(TransitionConditionBinaryExprKind.BoolAnd, l, r));

            return new TransitionConditionExpr.ConstExpr(true);
        }

        static TlaExpr MakeTlaExpr(this Transition transinion)
        {
            throw new NotImplementedException("");
        }
    }
}
