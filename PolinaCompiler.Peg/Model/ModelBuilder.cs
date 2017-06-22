using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg.Parser;

namespace PolinaCompiler.Peg.Model
{
    class ModelBuilder
    {
        class RuleInfoCollector : IParsingExpressionVisitor<object>
        {
            readonly ParsingRule _rule;
            readonly ModelClassInfo _class;

            int _quantorsCount = 0;

            private RuleInfoCollector(ParsingRule rule)
            {
                _rule = rule;
                _class = new ModelClassInfo(rule.Name);
            }

            private ModelClassInfo Collect()
            {
                _rule.Expr.Apply(this);
                return _class;
            }

            private void RegisterField(string name)
            {
                var field = _class.Fields.FirstOrDefault(f => f.Name == name);

                if (field == null)
                {
                    _class.Fields.Add(new ModelClassFieldInfo(name) {
                        IsCollection = _quantorsCount > 0
                    });
                }
                else
                {
                    field.IsCollection = true;
                }
            }

            #region IParsingExpressionVisitor<object> impl

            object IParsingExpressionVisitor<object>.VisitRuleCall(ParsingExpression.RuleCall ruleCall)
            {
                this.RegisterField(ruleCall.RuleName);
                return null;
            }

            object IParsingExpressionVisitor<object>.VisitChars(ParsingExpression.Characters characters)
            {
                this.RegisterField("@string");
                return null;
            }

            object IParsingExpressionVisitor<object>.VisitPattern(ParsingExpression.Pattern pattern)
            {
                this.RegisterField("@string");
                return null;
            }

            object IParsingExpressionVisitor<object>.VisitSeq(ParsingExpression.Sequence sequence)
            {
                foreach (var item in sequence.Childs)
                    item.Apply(this);
                
                return null;
            }

            object IParsingExpressionVisitor<object>.VisitAlts(ParsingExpression.Alternatives alternatives)
            {
                foreach (var item in alternatives.Childs)
                    item.Apply(this);

                return null;
            }

            object IParsingExpressionVisitor<object>.VisitNum(ParsingExpression.Number number)
            {
                if (number.Max > 1)
                    _quantorsCount++;

                number.Child.Apply(this);

                if (number.Max > 1)
                    _quantorsCount--;

                return null;
            }

            object IParsingExpressionVisitor<object>.VisitCheck(ParsingExpression.Check check)
            {
                // do nothing because this node describes only text checking, not actual parsing
                return null;
            }

            object IParsingExpressionVisitor<object>.VisitCheckNot(ParsingExpression.CheckNot checkNot)
            {
                // do nothing because this node describes only text checking, not actual parsing
                return null;
            }

            #endregion

            public static ModelClassInfo Collect(ParsingRule rule)
            {
                var collector = new RuleInfoCollector(rule);
                return collector.Collect();
            }
        }

        readonly ParsingGrammar _g;

        public ModelBuilder(ParsingGrammar g)
        {
            _g = g;
        }

        public ModelInfo Complete()
        {
            var model = new ModelInfo();
            model.Name = _g.StartRuleName;
            model.Namespace = _g.Name;
            
            foreach (var rule in _g)
            {
                var ruleClass = RuleInfoCollector.Collect(rule);
                model.Classes.Add(ruleClass);
            }

            model.Root = model.Classes.First(c => c.Name == _g.StartRuleName);

            return model;
        }
    }
}
