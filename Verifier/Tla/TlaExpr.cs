using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verifier.Tla
{
    public interface ITlaExprVisitor<TRet>
    {
        TRet VisitUntil(TlaExpr.Until until);
        TRet VisitRelease(TlaExpr.Release release);
        TRet VisitGlobally(TlaExpr.Globally globally);
        TRet VisitNext(TlaExpr.Next next);
        TRet VisitFuture(TlaExpr.Future future);
        TRet VisitNot(TlaExpr.Not not);
        TRet VisitAnd(TlaExpr.And and);
        TRet VisitImpl(TlaExpr.Impl impl);
        TRet VisitOr(TlaExpr.Or or);
        TRet VisitConst(TlaExpr.Const @const);
        TRet VisitVar(TlaExpr.Var var);
    }

    public abstract class TlaExpr
    {
        public TRet Apply<TRet>(ITlaExprVisitor<TRet> visitor)
        {
            return this.ApplyImpl(visitor);
        }

        protected abstract TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor);

        #region trivials

        public class Const : TlaExpr
        {
            public bool Value { get; private set; }

            public Const(bool value)
            {
                this.Value = value;
            }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitConst(this); }

            public override string ToString() { return this.Value.ToString(); }
        }

        public class Var : TlaExpr
        {
            public string Name { get; private set; }

            public Var(string name)
            {
                this.Name = name.LowerFirstCharacter();
            }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitVar(this); }

            public override string ToString() { return this.Name; }
        }

        #endregion

        #region abstracts 

        public abstract class BinaryExpression : TlaExpr
        {
            public TlaExpr Left { get; private set; }
            public TlaExpr Right { get; private set; }

            public BinaryExpression(TlaExpr left, TlaExpr right)
            {
                this.Left = left;
                this.Right = right;
            }
        }

        public abstract class UnaryExpression : TlaExpr
        {
            public TlaExpr Child { get; private set; }

            public UnaryExpression(TlaExpr child)
            {
                this.Child = child;
            }
        }

        #endregion

        #region impls

        public class Or : BinaryExpression
        {
            public Or(TlaExpr left, TlaExpr right) : base(left, right) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitOr(this); }

            public override string ToString() { return $"( ({Left}) || ({Right}) )"; }
        }

        public class Impl : BinaryExpression
        {
            public Impl(TlaExpr left, TlaExpr right) : base(left, right) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitImpl(this); }

            public override string ToString() { return $"( ({Left}) -> ({Right}) )"; }
        }

        public class And : BinaryExpression
        {
            public And(TlaExpr left, TlaExpr right) : base(left, right) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitAnd(this); }

            public override string ToString() { return $"(({Left}) && ({Right}))"; }
        }

        public class Not : UnaryExpression
        {
            public Not(TlaExpr proposition) : base(proposition) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitNot(this); }

            public override string ToString() { return $"!({Child})"; }
        }

        public class Future : UnaryExpression
        {
            public Future(TlaExpr proposition) : base(proposition) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitFuture(this); }

            public override string ToString() { return $"F({Child})"; }
        }

        public class Next : UnaryExpression
        {
            public Next(TlaExpr proposition) : base(proposition) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitNext(this); }

            public override string ToString() { return $"X({Child})"; }
        }

        public class Globally : UnaryExpression
        {
            public Globally(TlaExpr proposition) : base(proposition) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitGlobally(this); }

            public override string ToString() { return $"({Child})"; }
        }

        public class Release : BinaryExpression
        {
            public Release(TlaExpr left, TlaExpr right) : base(left, right) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitRelease(this); }

            public override string ToString() { return $"( ({Left}) R ({Right}) )"; }
        }

        public class Until : BinaryExpression
        {
            public Until(TlaExpr left, TlaExpr right) : base(left, right) { }

            protected override TRet ApplyImpl<TRet>(ITlaExprVisitor<TRet> visitor) { return visitor.VisitUntil(this); }

            public override string ToString() { return $"( ({Left}) U ({Right}) )"; }
        }

        #endregion
    }
}
