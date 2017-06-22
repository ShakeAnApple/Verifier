using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg.Parser;

namespace PolinaCompiler.Peg
{
    public struct StringFragment
    {
        readonly int _position, _length;
        string _text, _content;

        public int Position { get { return _position; } }
        public int Length { get { return _length; } }

        public string Content { get { return _content ?? (_content = _text.Substring(_position, _length)); } }

        public string FullText { get { return _text; } }

        public StringFragment(string text, int pos, int len)
        {
            if (pos < 0 || pos > text.Length)
                throw new ArgumentOutOfRangeException("pos");
            if (len < 0 || len > text.Length - pos)
                throw new ArgumentOutOfRangeException("len");

            _text = text;
            _position = pos;
            _length = len;
            _content = null;
        }

        public override string ToString()
        {
            return this.Content;
        }
    }

    public class StringTreeNode
    {
        public StringFragment Fragment { get; private set; }
        public ParsingRule Rule { get; private set; }
        public ReadOnlyCollection<StringTreeNode> Childs { get; private set; }

        public StringTreeNode(StringFragment fragment, ParsingRule rule, params StringTreeNode[] childs)
        {
            if (rule == null)
                throw new ArgumentNullException();

            // Console.WriteLine("{0}: {1}", rule.Name, string.Join(", ", childs.Select(c => c.Rule.Name)));

            this.Fragment = fragment;
            this.Rule = rule;
            this.Childs = new ReadOnlyCollection<StringTreeNode>(childs);
        }

        public override string ToString()
        {
            return string.Format("{0}@{1} {2}",
                this.Rule == null ? string.Empty : this.Rule.Name,
                this.Fragment.Position,
                this.Fragment.Content.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t")
            );
        }
    }
}
