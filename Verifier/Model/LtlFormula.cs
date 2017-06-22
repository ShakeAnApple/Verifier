//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Verifier.Model
//{
//    public class LtlFormula
//    {
//        public LtlFormula(LtlFormulaExpression root)
//        {
//            this.Root = root;
//        }
//        public LtlFormulaExpression Root { get; private set; }
//    }

//    public abstract class LtlFormulaExpression
//    {
//        public abstract LtlFormulaExpression Negate(LtlFormulaExpression node);

//        public abstract class BinaryExpression : LtlFormulaExpression
//        {
//            public LtlFormulaExpression Left { get; private set; }
//            public LtlFormulaExpression Right { get; private set; }

//            public BinaryExpression(LtlFormulaExpression left, LtlFormulaExpression right)
//            {
//                this.Left = left;
//                this.Right = right;
//            }
//        }

//        public abstract class UnaryExpression : LtlFormulaExpression
//        {
//            public LtlFormulaExpression Prop { get; private set; }
//            public UnaryExpression(LtlFormulaExpression proposition)
//            {
//                this.Prop = proposition;
//            }
//        }

//        public class Or : BinaryExpression
//        {
//            public Or(LtlFormulaExpression left, LtlFormulaExpression right) : base(left, right) { }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }

//            public override string ToString()
//            {
//                return $"( ({Left}) || ({Right}) )";
//            }
//        }

//        public class Impl : BinaryExpression
//        {
//            public Impl(LtlFormulaExpression left, LtlFormulaExpression right) : base(left, right){ }

//            public override string ToString()
//            {
//                return $"( ({Left}) -> ({Right}) )";
//            }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public class And : BinaryExpression
//        {
//            public And(LtlFormulaExpression left, LtlFormulaExpression right) : base(left, right){ }

//            public override string ToString()
//            {
//                return $"(({Left}) && ({Right}))";
//            }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }
//        }

//        public class Not : UnaryExpression
//        {
//            public Not(LtlFormulaExpression proposition) : base(proposition){ }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                return this.Prop;
//            }

//            public override string ToString()
//            {
//                return $"!({Prop})";
//            }
//        }

//        public class Future : UnaryExpression
//        {
//            public Future(LtlFormulaExpression proposition) : base(proposition){ }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }

//            public override string ToString()
//            {
//                return $"F({Prop})";
//            }
//        }

//        public class Next : UnaryExpression
//        {
//            public Next(LtlFormulaExpression proposition) : base(proposition){ }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }

//            public override string ToString()
//            {
//                return $"X({Prop})";
//            }
//        }

//        public class Globally : UnaryExpression
//        {
//            public Globally(LtlFormulaExpression proposition) : base(proposition){ }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }

//            public override string ToString()
//            {
//                return $"({Prop})";
//            }
//        }

//        public class Release : BinaryExpression
//        {
//            public Release(LtlFormulaExpression left, LtlFormulaExpression right) : base(left, right){ }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }

//            public override string ToString()
//            {
//                return $"( ({Left}) R ({Right}) )";
//            }
//        }

//        public class Until : BinaryExpression
//        {
//            public Until(LtlFormulaExpression left, LtlFormulaExpression right) : base(left, right){ }

//            public override LtlFormulaExpression Negate(LtlFormulaExpression node)
//            {
//                throw new NotImplementedException();
//            }

//            public override string ToString()
//            {
//                return $"( ({Left}) U ({Right}) )";
//            }
//        }
//    }
//}
