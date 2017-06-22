using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PolinaCompiler.Peg.Parser
{
    public interface IParsingExpressionVisitor<T>
    {
        T VisitRuleCall(ParsingExpression.RuleCall ruleCall);
        T VisitChars(ParsingExpression.Characters characters);
        T VisitPattern(ParsingExpression.Pattern pattern);
        T VisitSeq(ParsingExpression.Sequence sequence);
        T VisitAlts(ParsingExpression.Alternatives alternatives);
        T VisitNum(ParsingExpression.Number number);
        T VisitCheck(ParsingExpression.Check check);
        T VisitCheckNot(ParsingExpression.CheckNot checkNot);
    }

    public abstract class ParsingExpression
    {
        public ParsingState TryParse(ParsingState state)
        {
            if (!state.Skipping && !string.IsNullOrWhiteSpace(state.Grammar.SkipRuleName))
            {
                var next = state.Grammar.GetRule(state.Grammar.SkipRuleName).TryParse(state, true);
                if (next != null)
                    state = next;
            }

            return this.TryParseImpl(state);
        }

        protected abstract ParsingState TryParseImpl(ParsingState state);

        public T Apply<T>(IParsingExpressionVisitor<T> visitor)
        {
            return this.ApplyImpl(visitor);
        }

        protected abstract T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor);

        #region impls

        public class RuleCall : ParsingExpression
        {
            public string RuleName { get; private set; }

            public RuleCall(string ruleName)
            {
                this.RuleName = ruleName;
            }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                return state.Grammar.GetRule(this.RuleName).TryParse(state);
            }

            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitRuleCall(this);
            }

            public override string ToString()
            {
                return this.RuleName;
            }
        }

        public class Characters : ParsingExpression
        {
            public string Chars { get; private set; }

            public Characters(string chars)
            {
                this.Chars = chars;
            }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                if (state.Position + this.Chars.Length > state.Text.Length)
                    return null;

                for (int i = 0; i < this.Chars.Length; i++)
                    if (this.Chars[i] != state.Text[state.Position + i])
                        return null;

                return state.MoveForward(this.Chars.Length);
            }

            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitChars(this);
            }
            
            public override string ToString()
            {
                return "'" + this.Chars + "'";
            }
        }

        public class Pattern : ParsingExpression
        {
            public Regex Regex { get; private set; }
            public string ExprString { get; private set; }

            public Pattern(string pattern)
            {
                this.Regex = new Regex(pattern);
                this.ExprString = pattern;
            }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                var match = this.Regex.Match(state.Text, state.Position);
                if (!match.Success || match.Index != state.Position)
                    return null;

                return state.MoveForward(match.Length);
            }

            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitPattern(this);
            }
            
            public override string ToString()
            {
                return "\"" + this.ExprString + "\"";
            }
        }

        public abstract class ExprsGroup : ParsingExpression
        {
            public ReadOnlyCollection<ParsingExpression> Childs { get; private set; }

            public ExprsGroup(ParsingExpression[] childs)
            {
                this.Childs = new ReadOnlyCollection<ParsingExpression>(childs);
            }
        }

        public abstract class UnaryExpr : ParsingExpression
        {
            public ParsingExpression Child { get; private set; }

            public UnaryExpr(ParsingExpression child)
            {
                this.Child = child;
            }
        }

        public class Sequence : ExprsGroup
        {
            public Sequence(params ParsingExpression[] childs) : base(childs) { }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                var curr = state;

                foreach (var item in this.Childs)
                {
                    var next = item.TryParse(curr);
                    if (next == null)
                        return null;

                    curr = next;
                }

                return curr;
            }
            
            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitSeq(this);
            }

            public override string ToString()
            {
                return string.Join(" ", this.Childs);
            }
        }

        public class Alternatives : ExprsGroup
        {
            public Alternatives(params ParsingExpression[] childs) : base(childs) { }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                foreach (var item in this.Childs)
                {
                    var next = item.TryParse(state);
                    if (next != null)
                        return next;
                }

                return null;
            }

            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitAlts(this);
            }

            public override string ToString()
            {
                return string.Join(" | ", this.Childs);
            }
        }

        public class Number : UnaryExpr
        {
            public uint Min { get; private set; }
            public uint Max { get; private set; }

            public Number(uint min, uint max, ParsingExpression child)
                : base(child)
            {
                if (min < 0 || max < 0)
                    throw new ArgumentOutOfRangeException();

                this.Min = min;
                this.Max = max;
            }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                var curr = state;

                for (int i = 0; i < this.Min; i++)
                {
                    var next = this.Child.TryParse(curr);
                    if (next == null)
                        return null;

                    curr = next;
                }

                for (int i = 0; i < (this.Max - this.Min); i++)
                {
                    var next = this.Child.TryParse(curr);
                    if (next == null)
                        break;

                    curr = next;
                }

                return curr;
            }

            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitNum(this);
            }

            public override string ToString()
            {
                string q;
                if (this.Min == 0 && this.Max == uint.MaxValue)
                    q = "*";
                else if (this.Min == 1 && this.Max == uint.MaxValue)
                    q = "+";
                else if (this.Min == 0 && this.Max == 1)
                    q = "?";
                else if (this.Min == this.Max)
                    q = "{" + this.Min + "}";
                else if (this.Min == 0 && this.Max < uint.MaxValue)
                    q = "{" + "," + this.Max + "}";
                else if (this.Min > 0 && this.Max == uint.MaxValue)
                    q = "{" + this.Min + "," + "}";
                else
                    q = "{" + this.Min + "," + this.Max + "}";

                return this.Child + q;
            }
        }

        public class Check : UnaryExpr
        {
            public Check(ParsingExpression expr) : base(expr) { }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                var next = this.Child.TryParse(state);
                return next == null ? null : state;
            }

            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitCheck(this);
            }

            public override string ToString()
            {
                return "&" + this.Child;
            }
        }

        public class CheckNot : UnaryExpr
        {
            public CheckNot(ParsingExpression expr) : base(expr) { }

            protected override ParsingState TryParseImpl(ParsingState state)
            {
                var next = this.Child.TryParse(state);
                return next == null ? state : null;
            }

            protected override T ApplyImpl<T>(IParsingExpressionVisitor<T> visitor)
            {
                return visitor.VisitCheckNot(this);
            }

            public override string ToString()
            {
                return "^" + this.Child;
            }
        }

        #endregion
    }
}
