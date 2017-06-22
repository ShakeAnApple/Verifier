using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verifier.Model;

namespace Verifier.Tla
{
    public class TlaTransitionConditionFormula : TlaFormula
    {
        public TransitionConditionExpr Expression { get; private set; }

        public TlaTransitionConditionFormula(TransitionConditionExpr expr)
        {
            this.Expression = expr;
        }

        protected override bool IsConjunctionImpl()
        {
            return this.Expression.Apply(ConjunctionessChecker.Instance);
        }

        protected override SortedSet<string> GetAllVarsImpl()
        {
            return this.Expression.Apply(new VarsCollector());
        }

        protected override bool EvaluateImpl(Func<string, bool> varExpr)
        {
            return this.Expression.Apply(new Evaluator(varExpr));
        }

        public override string ToString()
        {
            return this.Expression.ToString();
        }

        class ConjunctionessChecker : ITransitionConditionExprVisitor<bool>
        {
            private ConjunctionessChecker() { }

            public static readonly ConjunctionessChecker Instance = new ConjunctionessChecker();

            bool ITransitionConditionExprVisitor<bool>.VisitBinary(TransitionConditionExpr.BinaryExpr bin)
            {
                return bin.Kind == TransitionConditionBinaryExprKind.BoolAnd && bin.Left.Apply(this) && bin.Right.Apply(this);
            }

            bool ITransitionConditionExprVisitor<bool>.VisitNot(TransitionConditionExpr.NotExpr not)
            {
                return false;
            }

            bool ITransitionConditionExprVisitor<bool>.VisitVar(TransitionConditionExpr.VarExpr var)
            {
                return true;
            }

            bool ITransitionConditionExprVisitor<bool>.VisitConst(TransitionConditionExpr.ConstExpr constExpr)
            {
                return true;
            }
        }

        class VarsCollector : SortedSet<string>, ITransitionConditionExprVisitor<VarsCollector>
        {
            VarsCollector ITransitionConditionExprVisitor<VarsCollector>.VisitBinary(TransitionConditionExpr.BinaryExpr bin)
            {
                if (bin.Kind != TransitionConditionBinaryExprKind.BoolAnd)
                    throw new InvalidOperationException();

                bin.Left.Apply(this);
                bin.Right.Apply(this);

                return this;
            }

            VarsCollector ITransitionConditionExprVisitor<VarsCollector>.VisitConst(TransitionConditionExpr.ConstExpr constExpr)
            {
                return this;
            }

            VarsCollector ITransitionConditionExprVisitor<VarsCollector>.VisitNot(TransitionConditionExpr.NotExpr not)
            {
                throw new InvalidOperationException();
            }

            VarsCollector ITransitionConditionExprVisitor<VarsCollector>.VisitVar(TransitionConditionExpr.VarExpr var)
            {
                this.Add(var.Name);
                return this;
            }
        }

        class Evaluator : ITransitionConditionExprVisitor<bool>
        {
            readonly Func<string, bool> _varEvaluator;

            public Evaluator(Func<string, bool> varEvaluator)
            {
                _varEvaluator = varEvaluator;
            }

            bool ITransitionConditionExprVisitor<bool>.VisitBinary(TransitionConditionExpr.BinaryExpr bin)
            {
                switch (bin.Kind)
                {
                    case TransitionConditionBinaryExprKind.BoolOr: return bin.Left.Apply(this) || bin.Right.Apply(this);
                    case TransitionConditionBinaryExprKind.BoolAnd: return bin.Left.Apply(this) && bin.Right.Apply(this);
                    default:
                        throw new NotImplementedException("");
                }
            }

            bool ITransitionConditionExprVisitor<bool>.VisitConst(TransitionConditionExpr.ConstExpr constExpr)
            {
                return constExpr.Value;
            }

            bool ITransitionConditionExprVisitor<bool>.VisitNot(TransitionConditionExpr.NotExpr not)
            {
                return !not.Child.Apply(this);
            }

            bool ITransitionConditionExprVisitor<bool>.VisitVar(TransitionConditionExpr.VarExpr var)
            {
                return _varEvaluator(var.Name);
            }
        }
    }
}
