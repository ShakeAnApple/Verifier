using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verifier.Tla
{
    class TlaExprFormula : TlaFormula
    {
        public TlaExpr Expression { get; private set; }

        public TlaExprFormula(TlaExpr expr)
        {
            this.Expression = expr;
        }

        protected override bool IsConjunctionImpl()
        {
            throw new NotImplementedException();
        }

        protected override SortedSet<string> GetAllVarsImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool EvaluateImpl(Func<string, bool> varExpr)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.Expression.ToString();
        }
    }
}
