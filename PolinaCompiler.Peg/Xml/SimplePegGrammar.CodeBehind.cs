 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg.Parser;

namespace PolinaCompiler.Peg.Xml
{
    partial class GrammarType
    {
        public GrammarType() { }

        public GrammarType(ParsingGrammar g)
        {
            this.SkipRule = g.SkipRuleName;
            this.StartRule = g.StartRuleName;
            this.Items = g.Select(r => new RuleType(r)).ToArray();
        }

        public ParsingGrammar ToGrammar()
        {
            var g = new ParsingGrammar() {
                StartRuleName = this.StartRule,
                SkipRuleName = this.SkipRule
            };

            foreach (var rule in this.Items)
                g.Add(rule.Name, rule.Item.Apply(XmlExprTypeToParsingExpressionConverter.Instance));

            return g;
        }
    }

    partial class RuleType
    {
        public RuleType() { }

        public RuleType(ParsingRule r)
        {
            this.Name = r.Name;

            this.Item = r.Expr.Apply(ParsingExpressionToXmlExprTypeConverter.Instance);
        }
    }

    public interface IXmlExprTypeVisitor<T>
    {
        T VisitCall(ExprCallType expr);
        T VisitChars(ExprCharsType expr);
        T VisitRegex(ExprPatternType expr);
        T VisitCheck(ExprCheckType expr);
        T VisitCheckNot(ExprCheckNotType expr);
        T VisitSeq(ExprSequenceType expr);
        T VisitAlts(ExprAlternativesType expr);
        T VisitNumber(ExprNumberType expr);
    }

    partial class ExpressionType
    {
        public T Apply<T>(IXmlExprTypeVisitor<T> visitor)
        {
            return this.ApplyImpl<T>(visitor);
        }

        protected abstract T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor);
    }

    partial class ExprCallType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitCall(this); } }
    partial class ExprCharsType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitChars(this); } }
    partial class ExprPatternType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitRegex(this); } }
    partial class ExprCheckType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitCheck(this); } }
    partial class ExprCheckNotType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitCheckNot(this); } }
    partial class ExprSequenceType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitSeq(this); } }
    partial class ExprAlternativesType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitAlts(this); } }
    partial class ExprNumberType { protected override T ApplyImpl<T>(IXmlExprTypeVisitor<T> visitor) { return visitor.VisitNumber(this); } }

}
