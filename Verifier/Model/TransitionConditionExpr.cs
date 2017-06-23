using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verifier.Tla;

namespace Verifier.Model
{
    public enum TransitionConditionBinaryExprKind
    {
        BoolOr,
        BoolAnd
    }

    public interface ITransitionConditionExprVisitor<TRet>
    {
        TRet VisitBinary(TransitionConditionExpr.BinaryExpr bin);
        TRet VisitNot(TransitionConditionExpr.NotExpr not);
        TRet VisitVar(TransitionConditionExpr.VarExpr var);
        TRet VisitConst(TransitionConditionExpr.ConstExpr constExpr);
    }

    public abstract class TransitionConditionExpr
    {
        public TRet Apply<TRet>(ITransitionConditionExprVisitor<TRet> visitor)
        {
            return this.ApplyImpl(visitor);
        }

        protected abstract TRet ApplyImpl<TRet>(ITransitionConditionExprVisitor<TRet> visitor);

        #region impls

        public class VarExpr : TransitionConditionExpr
        {
            public string Name { get; private set; }

            public VarExpr(string name)
            {
                this.Name = name; //.LowerFirstCharacter();
            }

            protected override TRet ApplyImpl<TRet>(ITransitionConditionExprVisitor<TRet> visitor)
            {
                return visitor.VisitVar(this);
            }

            public override string ToString()
            {
                return this.Name;
            }
        }

        public class ConstExpr : TransitionConditionExpr
        {
            public bool Value { get; private set; }

            public ConstExpr(bool value)
            {
                this.Value = value;
            }

            protected override TRet ApplyImpl<TRet>(ITransitionConditionExprVisitor<TRet> visitor)
            {
                return visitor.VisitConst(this);
            }

            public override string ToString()
            {
                return this.Value.ToString();
            }
        }

        public class NotExpr : TransitionConditionExpr
        {
            public TransitionConditionExpr Child { get; private set; }

            public NotExpr(TransitionConditionExpr child)
            {
                this.Child = child;
            }

            protected override TRet ApplyImpl<TRet>(ITransitionConditionExprVisitor<TRet> visitor)
            {
                return visitor.VisitNot(this);
            }

            public override string ToString()
            {
                return string.Format("!{0}", this.Child);
            }
        }

        public class BinaryExpr : TransitionConditionExpr
        {
            public TransitionConditionExpr Left { get; private set; }
            public TransitionConditionExpr Right { get; private set; }
            public TransitionConditionBinaryExprKind Kind { get; private set; }

            public BinaryExpr(TransitionConditionBinaryExprKind kind, TransitionConditionExpr left, TransitionConditionExpr right)
            {
                this.Left = left;
                this.Right = right;
                this.Kind = kind;
            }

            protected override TRet ApplyImpl<TRet>(ITransitionConditionExprVisitor<TRet> visitor)
            {
                return visitor.VisitBinary(this);
            }

            public override string ToString()
            {
                var ops = new Dictionary<TransitionConditionBinaryExprKind, string>() {
                    { TransitionConditionBinaryExprKind.BoolAnd, "&&" },
                    { TransitionConditionBinaryExprKind.BoolOr, "||" },
                };

                return string.Format("({0} {1} {2})", this.Left, ops[this.Kind], this.Right);
            }
        }

        #endregion
    }

}
