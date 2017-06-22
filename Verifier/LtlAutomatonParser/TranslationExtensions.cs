using PolinaCompiler.Peg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verifier.LtlAutomatonParser.TextModel;
using Verifier.Model;

namespace Verifier.LtlAutomatonParser
{
    static class TranslationExtensions
    {
        static readonly Dictionary<string, TransitionConditionBinaryExprKind> _opsByStr = new Dictionary<string, TransitionConditionBinaryExprKind>() {
            { "||", TransitionConditionBinaryExprKind.BoolOr },
            { "&&", TransitionConditionBinaryExprKind.BoolAnd },
        };

        static readonly Dictionary<TransitionConditionBinaryExprKind, int> _priorityByOp = new Dictionary<TransitionConditionBinaryExprKind, int>() {
            { TransitionConditionBinaryExprKind.BoolOr, 1 },
            { TransitionConditionBinaryExprKind.BoolAnd, 2 },
        };

        public static Automaton TranslateToModel(this automaton root)
        {
            var transitionIdCount = 0;

            var a = new Automaton() {
                States = root.states.Select((s, n) => new State(n) {
                    Name = s.stateName.identifier.@string,
                    IsInitial = s.stateName.identifier.@string.EndsWith("_init"),
                    IsAccepting = s.stateName.identifier.@string.StartsWith("accept_"),
                }).ToList()
            };

            foreach (var state in root.states)
            {
                var st = a.States.First(s => s.Name == state.stateName.identifier.@string);

                st.Outgoing = state.transitions != null ? state.transitions.Select(t => new Transition(transitionIdCount++) {
                    ToId = a.States.First(s => s.Name == t.stateName.identifier.@string).Id,
                    FromId = st.Id,
                    Condition = t.condition.exprGroup.exprSeq.TranslateToConditionExpr()
                }).ToList() : new List<Transition>();
            }

            foreach (var st in a.States)
            {
                st.Incoming = a.States.SelectMany(s => s.Outgoing).Where(t => t.ToId == st.Id).ToList();
            }

            return a;
        }

        class ExprItem
        {
            public TransitionConditionBinaryExprKind? Op { get; private set; }
            public TransitionConditionExpr Expr { get; private set; }
            public int Priority { get; private set; }

            public ExprItem(TransitionConditionBinaryExprKind op)
            {
                this.Op = op;
                this.Priority = _priorityByOp[op];
                this.Expr = null;
            }

            public ExprItem(TransitionConditionExpr expr)
            {
                this.Op = null;
                this.Priority = int.MaxValue;
                this.Expr = expr;
            }
        }

        public static TransitionConditionExpr TranslateToConditionExpr(this exprSeq expr)
        {
            if (expr.exprItems.Length != expr.boolOperators.Length + 1)
                throw new ApplicationException();

            var items = new ExprItem[expr.exprItems.Length + expr.boolOperators.Length];
            items[0] = new ExprItem(expr.exprItems[0].Translate());
            for (int i = 0, j = 1; i < expr.boolOperators.Length; i++, j += 2)
            {
                items[j + 0] = new ExprItem(_opsByStr[expr.boolOperators[i].strings.First()]);
                items[j + 1] = new ExprItem(expr.exprItems[i + 1].Translate());
            }

            return TranslateExprPart(items, 0, items.Length);
        }

        private static TransitionConditionExpr TranslateExprPart(ExprItem[] items, int from, int to)
        {
            if (from == to - 1)
            {
                var e = items[from].Expr;
                if (e == null)
                    throw new InvalidProgramException();

                return e;
            }

            var indexOfMinPriority = -1;
            var minPriority = int.MaxValue;

            for (int i = from; i < to; i++)
            {
                var it = items[i];
                if (it.Priority <= minPriority)
                {
                    indexOfMinPriority = i;
                    minPriority = it.Priority;
                }
            }

            return new TransitionConditionExpr.BinaryExpr(
                items[indexOfMinPriority].Op.Value,
                TranslateExprPart(items, from, indexOfMinPriority),
                TranslateExprPart(items, indexOfMinPriority + 1, to)
            );
        }

        private static TransitionConditionExpr Translate(this exprItem item)
        {
            TransitionConditionExpr expr;

            if (item.identifier != null)
                expr = new TransitionConditionExpr.VarExpr(item.identifier.@string);
            else if (item.exprGroup != null)
                expr = item.exprGroup.exprSeq.TranslateToConditionExpr();
            else if (item.literal != null)
                expr = new TransitionConditionExpr.ConstExpr(true);
            else
                throw new NotImplementedException();

            if (item.not != null)
                expr = new TransitionConditionExpr.NotExpr(expr);

            return expr;
        }
    }
}
