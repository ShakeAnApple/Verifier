using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg.Parser;

namespace PolinaCompiler.Peg.Xml
{
    class ParsingExpressionToXmlExprTypeConverter : IParsingExpressionVisitor<ExpressionType>
    {
        public static readonly ParsingExpressionToXmlExprTypeConverter Instance = new ParsingExpressionToXmlExprTypeConverter();

        #region IParsingExpressionVisitor<ExpressionType> impl

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitRuleCall(ParsingExpression.RuleCall ruleCall)
        {
            return new ExprCallType() { RuleName = ruleCall.RuleName };
        }

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitChars(ParsingExpression.Characters characters)
        {
            return new ExprCharsType() { String = characters.Chars };
        }

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitPattern(ParsingExpression.Pattern pattern)
        {
            return new ExprPatternType() { Pattern = pattern.ExprString };
        }

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitSeq(ParsingExpression.Sequence sequence)
        {
            return new ExprSequenceType() { Items = sequence.Childs.Select(c => c.Apply(this)).ToArray() };
        }

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitAlts(ParsingExpression.Alternatives alternatives)
        {
            return new ExprAlternativesType() { Items = alternatives.Childs.Select(c => c.Apply(this)).ToArray() };
        }

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitNum(ParsingExpression.Number number)
        {
            var num = new ExprNumberType() { Item = number.Child.Apply(this) };

            if (number.Max < int.MaxValue)
            {
                num.Max = number.Max;
                num.MaxSpecified = true;
            }

            if (number.Min > 0)
            {
                num.Min = number.Min;
                num.MinSpecified = true;
            }

            return num;
        }

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitCheck(ParsingExpression.Check check)
        {
            return new ExprCheckType() { Item = check.Child.Apply(this) };
        }

        ExpressionType IParsingExpressionVisitor<ExpressionType>.VisitCheckNot(ParsingExpression.CheckNot checkNot)
        {
            return new ExprCheckNotType() { Item = checkNot.Child.Apply(this) };
        }

        #endregion
    }

    class XmlExprTypeToParsingExpressionConverter : IXmlExprTypeVisitor<ParsingExpression>
    {
        public static readonly XmlExprTypeToParsingExpressionConverter Instance = new XmlExprTypeToParsingExpressionConverter();

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitAlts(ExprAlternativesType expr)
        {
            return new ParsingExpression.Alternatives(expr.Items.Select(it => it.Apply(this)).ToArray());
        }

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitCall(ExprCallType expr)
        {
            return new ParsingExpression.RuleCall(expr.RuleName);
        }

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitChars(ExprCharsType expr)
        {
            return new ParsingExpression.Characters(expr.String);
        }

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitCheck(ExprCheckType expr)
        {
            return new ParsingExpression.Check(expr.Item.Apply(this));
        }

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitCheckNot(ExprCheckNotType expr)
        {
            return new ParsingExpression.CheckNot(expr.Item.Apply(this));
        }

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitNumber(ExprNumberType expr)
        {
            uint min = 0, max = uint.MaxValue;

            if (expr.MinSpecified)
                min = expr.Min;

            if (expr.MaxSpecified)
                max = expr.Max;

            return new ParsingExpression.Number(min, max, expr.Item.Apply(this));
        }

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitRegex(ExprPatternType expr)
        {
            return new ParsingExpression.Pattern(expr.Pattern);
        }

        ParsingExpression IXmlExprTypeVisitor<ParsingExpression>.VisitSeq(ExprSequenceType expr)
        {
            return new ParsingExpression.Sequence(expr.Items.Select(it => it.Apply(this)).ToArray());
        }
    }
}
