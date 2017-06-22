using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using PolinaCompiler.Peg.Xml;

namespace PolinaCompiler.Peg.Parser
{
    public class ParsingGrammar : IEnumerable<ParsingRule>
    {
        readonly Dictionary<string, ParsingRule> _rules = new Dictionary<string, ParsingRule>();

        public string Name { get; set; }
        public string StartRuleName { get; set; }
        public string SkipRuleName { get; set; }
        public IndentedWriter LastLog { get; private set; }

        public ParsingGrammar()
        {
        }

        public void Add(string name, ParsingExpression expr)
        {
            _rules.Add(name, new ParsingRule(name, expr));
        }

        public ParsingState TryParse(string text)
        {
            var rootRule = _rules[this.StartRuleName];
            var state = new ParsingState(this, rootRule, text);
            this.LastLog = state.Log;

            var last = rootRule.TryParse(state);

            if (last != null && !string.IsNullOrWhiteSpace(this.SkipRuleName))
            {
                var next = this.GetRule(this.SkipRuleName).TryParse(last, true);
                if (next != null)
                    last = next;
            }

            return last;
        }

        public ParsingRule GetRule(string ruleName)
        {
            return _rules[ruleName];
        }

        public string GetDefinitionText()
        {
            return _rules.Values.Aggregate(new StringBuilder(), (sb, r) => sb.AppendLine(r.ToString())).ToString();
        }

        #region IEnumerator<ParsingRule> impl

        public IEnumerator<ParsingRule> GetEnumerator()
        {
            return _rules.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
