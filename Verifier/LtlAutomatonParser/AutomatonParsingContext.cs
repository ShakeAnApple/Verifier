using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Verifier.LtlAutomatonParser
{
    public class AutomatonParsingContext
    {
        static readonly Regex _namesRegex = new Regex(@"^(?<text>[\W]*)(((?<name>(?![UWVX]\W)[\w][\w]*)|(?<text>[UWVX]))(?<text>[\W]*))*$");

        public string RawLtl { get; private set; }
        public string EscapedLtl { get; private set; }

        readonly Dictionary<string, string> _escapedByOriginal;
        readonly Dictionary<string, string> _originalByEscaped;

        public AutomatonParsingContext(string ltl)
        {
            var match = _namesRegex.Match(ltl);
            if (!match.Success)
                throw new ApplicationException("ltl automaton context construction failed");

            var n = 0;
            _originalByEscaped = match.Groups["name"].Captures.Cast<Capture>()
                                      .Select(c => c.Value).Distinct()
                                      .ToDictionary(s => "e" + (++n) + "_" + s.ToLower(), s => s);

            _escapedByOriginal = _originalByEscaped.ToDictionary(kv => kv.Value, kv => kv.Key);

            this.EscapedLtl = string.Join(string.Empty,
                match.Groups["name"].Captures.Cast<Capture>().Concat(match.Groups["text"].Captures.Cast<Capture>())
                     .OrderBy(c => c.Index).Select(c => c.Value).Select(s => this.EscapeOrDefault(s, s))
            );
        }

        public string EscapeOrDefault(string name, string @default)
        {
            if (name == "true" || name == "false" || name == "1")
                return @default;

            string result;
            return _escapedByOriginal.TryGetValue(name, out result) ? result : @default;
        }

        public string Unescape(string name)
        {
            return _originalByEscaped[name];
        }
    }
}
