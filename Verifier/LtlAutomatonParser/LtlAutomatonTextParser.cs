using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg.Parser;
using PolinaCompiler.Peg.Def;
using Verifier.LtlAutomatonParser.TextModel;
using Verifier.Model;
using Verifier.Tla;

namespace Verifier.LtlAutomatonParser
{
    public class LtlAutomatonTextParser
    {
        ParsingGrammar _grammar;

        public LtlAutomatonTextParser()
        {
            _grammar = DefenitionsGrammar.TryParse(LtlParserResources.SourceGrammarText);
        }

        private automaton TryParseText(string text)
        {
            var state = _grammar.TryParse(text);

            if (state.Position < text.Length)
                return null;

            var textTree = state.CurrentNodes.First();

            var tree = TextModel.SourceAutomatonMapping.MapAutomaton(textTree);
            if (tree == null)
                return null;

            if (tree.states.Any(s => s.errTransition != null))
                throw new ApplicationException("invalid ltl automaton");

            return tree;
        }

        public Automaton TryParseModelAutomaton(string text)
        {
            var tree = this.TryParseText(text);
            return tree?.TranslateToModel();
        }

        public TlaAutomaton TryParseTlaAutomaton(string text, bool useTransitionConditions)
        {
            var tree = this.TryParseText(text);
            return tree?.TranslateToTlaAutomaton(useTransitionConditions);
        }
    }
}
