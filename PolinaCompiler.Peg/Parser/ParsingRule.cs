using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolinaCompiler.Peg.Parser
{
    public class ParsingRule
    {
        public string Name { get; private set; }
        public ParsingExpression Expr { get; private set; }

        public ParsingRule(string name, ParsingExpression expr)
        {
            this.Name = name;
            this.Expr = expr;
        }

        public ParsingState TryParse(ParsingState state, bool skipping = false)
        {
            if (!skipping)
                state.Log.WriteLine("enter {0} @{1} on {2}", this.Name, state.Position, state.Position < state.Text.Length ? state.Text[state.Position].ToString() : "EOT").Push();

            var next = this.Expr.TryParse(state.EnterRule(this, skipping));

            if (!skipping)
                if (next == null)
                    state.Log.WriteLine("drop {0} @{1}", this.Name, state.Position).Pop();
                else
                    state.Log.WriteLine("capture {0} @{1}-{2}: {3}", this.Name, state.Position, next.Position, next.Text.Substring(state.Position, next.Position - state.Position)).Pop();

            //if (next != null && next.Position == 13)
            //    Console.WriteLine();

            //if (next != null)
            //    state.Log.WriteLine("{0}", next.CurrentNodes.Count);

            return next == null ? null : next.ExitRule();
        }

        public override string ToString()
        {
            return this.Name + ": " + this.Expr + ";";
        }
    }

}
