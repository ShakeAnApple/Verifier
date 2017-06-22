using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg.Parser;
using PolinaCompiler.Peg.Def.TextModel;

namespace PolinaCompiler.Peg.Def
{
    public class DefenitionsGrammar
    {
        static readonly ParsingGrammar _grammar;

        static DefenitionsGrammar()
        {
            var loader = new Xml.GrammarXmlSerializer();
            _grammar = loader.LoadFromXml(PegResources.defXml);
        }

        public static ParsingGrammar TryParse(string text)
        {
            var state = _grammar.TryParse(text);
            var textTree = state.CurrentNodes.First();

            var grammar = SourceGrammarMapping.MapGrammar(textTree);

            return Translate(grammar);
        }

        private static ParsingGrammar Translate(grammar grammar)
        {
            var g = new ParsingGrammar();

            if (grammar.header.nsName != null)
                g.Name = string.Join(".", grammar.header.nsName.identifiers.Select(s => s.@string));

            g.StartRuleName = grammar.header.identifiers.First().@string;

            if (grammar.header.identifiers.Length > 1)
                g.SkipRuleName = grammar.header.identifiers.Last().@string;

            foreach (var r in grammar.rules)
            {
                g.Add(r.identifier.@string, TranslateSeq(r.exprsSeq));
            }

            return g;
        }

        private static ParsingExpression TranslateSeq(exprsSeq seq)
        {
            return new ParsingExpression.Sequence(seq.exprItems.Select(it => TranslateSeqItem(it)).ToArray());
        }

        private static ParsingExpression TranslateSeqItem(exprItem seq)
        {
            if (seq.alternatives != null)
                return new ParsingExpression.Alternatives(seq.alternatives.altItems.Select(it => TranslateAltItem(it)).ToArray());
            else if (seq.number != null)
                return TranslateNumber(seq.number);
            else if (seq.trivial != null)
                return TranslateTrivial(seq.trivial);
            else
                throw new NotImplementedException("");
        }

        private static ParsingExpression TranslateAltItem(altItem seq)
        {
            if (seq.number != null)
                return TranslateNumber(seq.number);
            else if (seq.trivial != null)
                return TranslateTrivial(seq.trivial);
            else
                throw new NotImplementedException("");
        }

        private static ParsingExpression TranslateNumber(number num)
        {
            uint min = 0, max = uint.MaxValue;

            if (num.quantor.strings.Any(s => s == "+")) min = 1;
            else if (num.quantor.strings.Any(s => s == "?")) max = 1;
            else if (num.quantor.strings.Any(s => s == "*")) ; // ok
            else
            {
                throw new NotImplementedException("");
            }

            return new ParsingExpression.Number(min, max, TranslateTrivial(num.trivial));
        }

        private static ParsingExpression TranslateTrivial(trivial e)
        {
            if (e.chars != null)
                return new ParsingExpression.Characters(e.chars.@string.Substring(1, e.chars.@string.Length - 2));
            else if (e.check != null)
                return new ParsingExpression.Check(TranslateTrivial(e.check.trivial));
            else if (e.not != null)
                return new ParsingExpression.CheckNot(TranslateTrivial(e.not.trivial));
            else if (e.group != null)
                return TranslateSeq(e.group.exprsSeq);
            else if (e.identifier != null)
                return new ParsingExpression.RuleCall(e.identifier.@string);
            else if (e.regex != null)
                return new ParsingExpression.Pattern(e.regex.@string.Substring(1, e.regex.@string.Length - 2));
            else
                throw new NotImplementedException("");
        }


    }
}
