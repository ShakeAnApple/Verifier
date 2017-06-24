using PolinaCompiler.Peg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verifier.LtlAutomatonParser.TextModel;
using Verifier.Tla;

namespace Verifier.LtlAutomatonParser
{
    static class TlaTranslationExtensions
    {
        enum BinaryOps
        {
            BoolOr,
            BoolAnd,
            Impl,
            Release,
            Until
        }

        // TODO - define them in grammar, symbols and priorities

        static readonly Dictionary<string, BinaryOps> _opsByStr = new Dictionary<string, BinaryOps>() {
            { "||", BinaryOps.BoolOr },
            { "&&", BinaryOps.BoolAnd },
        };

        static readonly Dictionary<BinaryOps, int> _priorityByOp = new Dictionary<BinaryOps, int>() {
            { BinaryOps.BoolOr, 1 },
            { BinaryOps.BoolAnd, 2 },
        };

        static readonly Dictionary<BinaryOps, Func<TlaExpr, TlaExpr, TlaExpr>> _ctorsByOp = new Dictionary<BinaryOps, Func<TlaExpr, TlaExpr, TlaExpr>>() {
            { BinaryOps.BoolOr,  (a,b) => new TlaExpr.Or(a, b) },
            { BinaryOps.BoolAnd, (a,b) => new TlaExpr.And(a, b) },
            { BinaryOps.Impl,    (a,b) => new TlaExpr.Impl(a, b) },
            { BinaryOps.Release, (a,b) => new TlaExpr.Release(a, b) },
            { BinaryOps.Until,   (a,b) => new TlaExpr.Until(a, b) },
        };

        public static TlaAutomaton TranslateToTlaAutomaton(this automaton root, bool useTransitionConditions, AutomatonParsingContext ctx)
        {
            var automaton = new TlaAutomaton();

            foreach (var state in root.states)
            {
                var name = state.stateName.identifier.@string;
                automaton.CreateState(name, name.EndsWith("_init"), name.StartsWith("accept_"));
            }

            foreach (var state in root.states)
            {
                foreach (var target in state.transitions)
                {
                    var condition = target.condition.Translate(useTransitionConditions, ctx);
                    automaton.CreateTransition(state.stateName.identifier.@string, target.stateName.identifier.@string, condition);
                }

                if (state.skip != null)
                {
                    automaton.CreateTransition(
                        state.stateName.identifier.@string, state.stateName.identifier.@string,
                        useTransitionConditions ? new TlaTransitionConditionFormula(new Model.TransitionConditionExpr.ConstExpr(true))
                                                : (TlaFormula)new TlaExprFormula(new TlaExpr.Const(true))
                    );
                }
            }

            return automaton;
        }

        static TlaFormula Translate(this condition cond, bool useTransitionConditions, AutomatonParsingContext ctx)
        {
            TlaFormula result;
            if (useTransitionConditions)
                result = new TlaTransitionConditionFormula(cond.exprSeq.TranslateToConditionExpr(ctx));
            else
                result = new TlaExprFormula(cond.exprSeq.TranslateToTlaExpr(ctx));

            return result;
        }

        class ExprItem
        {
            public BinaryOps? Op { get; private set; }
            public TlaExpr Expr { get; private set; }
            public int Priority { get; private set; }

            public ExprItem(BinaryOps op)
            {
                this.Op = op;
                this.Priority = _priorityByOp[op];
                this.Expr = null;
            }

            public ExprItem(TlaExpr expr)
            {
                this.Op = null;
                this.Priority = int.MaxValue;
                this.Expr = expr;
            }
        }

        private static TlaExpr TranslateToTlaExpr(this exprSeq expr, AutomatonParsingContext ctx)
        {
            if (expr.exprItems.Length != expr.boolOperators.Length + 1)
                throw new ApplicationException();

            var items = new ExprItem[expr.exprItems.Length + expr.boolOperators.Length];
            items[0] = new ExprItem(expr.exprItems[0].Translate(ctx));
            for (int i = 0, j = 1; i < expr.boolOperators.Length; i++, j += 2)
            {
                items[j + 0] = new ExprItem(_opsByStr[expr.boolOperators[i].strings.First()]);
                items[j + 1] = new ExprItem(expr.exprItems[i + 1].Translate(ctx));
            }

            return TranslateExprPart(items, 0, items.Length);
        }

        private static TlaExpr TranslateExprPart(ExprItem[] items, int from, int to)
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

            var binExprCtor = _ctorsByOp[items[indexOfMinPriority].Op.Value];

            var left = TranslateExprPart(items, from, indexOfMinPriority);
            var right = TranslateExprPart(items, indexOfMinPriority + 1, to);

            return binExprCtor(left, right);
        }

        private static TlaExpr Translate(this exprItem item, AutomatonParsingContext ctx)
        {
            TlaExpr expr;

            if (item.identifier != null)
            {
                bool constValue;

                if (bool.TryParse(item.identifier.@string, out constValue))
                    expr = new TlaExpr.Const(constValue);
                else
                    expr = new TlaExpr.Var(ctx.Unescape(item.identifier.@string));
            }
            else if (item.exprGroup != null)
            {
                expr = item.exprGroup.exprSeq.TranslateToTlaExpr(ctx);
            }
            else
            {
                throw new NotImplementedException();
            }

            if (item.not != null)
                expr = new TlaExpr.Not(expr);

            return expr;
        }
    }
}
