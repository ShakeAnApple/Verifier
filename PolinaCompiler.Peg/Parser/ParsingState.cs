using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace PolinaCompiler.Peg.Parser
{
    public class ParsingState
    {
        public ParsingGrammar Grammar { get; private set; }

        public string Text { get; private set; }
        public int Position { get; private set; }

        public ParsingState Parent { get; private set; }
        public ParsingState Prev { get; private set; }

        // public StringTreeNode CurrNode { get; private set; }

        public ReadOnlyCollection<StringTreeNode> CurrentNodes { get; private set; }
        public ParsingRule CurrentRule { get; private set; }

        public IndentedWriter Log { get; private set; }
        public int Depth { get; private set; }
        public bool Skipping { get; private set; }

        public ParsingState(ParsingGrammar grammar, ParsingRule rule, string text)
        {
            this.Grammar = grammar;
            this.Text = text;
            this.Position = 0;
            this.Prev = null;
            this.CurrentNodes = new ReadOnlyCollection<StringTreeNode>(new StringTreeNode[0]);
            this.CurrentRule = rule;
            this.Log = new IndentedWriter(" ");
            this.Parent = new ParsingState(this, null, null, false);
            this.Skipping = false;
        }

        private ParsingState(ParsingState prev, ParsingRule rule, ParsingState parent, int pos, bool skipping, IEnumerable<StringTreeNode> nodes)
        {
            this.Grammar = prev.Grammar;
            this.Text = prev.Text;
            this.Log = prev.Log;

            this.Position = pos;
            this.Skipping = skipping;
            this.Prev = prev;
            this.CurrentRule = rule;
            this.CurrentNodes = new ReadOnlyCollection<StringTreeNode>(nodes.ToArray());
            this.Parent = parent;
        }

        public ParsingState(ParsingState prev, ParsingState parent, ParsingRule parsingRule, bool skipping)
        {
            this.Grammar = prev.Grammar;
            this.Text = prev.Text;
            this.Log = prev.Log;
            this.Position = prev.Position;

            this.Skipping = skipping;
            this.Prev = prev;
            this.CurrentNodes = new ReadOnlyCollection<StringTreeNode>(new StringTreeNode[0]);
            this.CurrentRule = parsingRule;
            this.Parent = parent;
        }

        public ParsingState MoveForward(int len)
        {
            var fragment = new StringFragment(this.Text, this.Position, len);   
            this.Log.WriteLine("fwd@{0} {1}", this.Position, fragment);

            // Console.WriteLine("fwd in " + this.CurrentRule.Name + " @" + this.Position);

            return new ParsingState(
                this,
                this.CurrentRule,
                this.Parent,
                this.Position + len,
                this.Skipping,
                this.CurrentNodes.Concat(new[] {
                    new StringTreeNode(fragment, this.CurrentRule)
                })
            ) { Depth = this.Depth };
        }

        public ParsingState EnterRule(ParsingRule parsingRule, bool skipping)
        {
            if (this.Depth > 100)
                throw new ApplicationException("Recursion guard");

            // Console.WriteLine("enter in " + parsingRule.Name + " @" + this.Position);
            return new ParsingState(this, this, parsingRule, skipping) { Depth = this.Depth + 1 };
        }

        public ParsingState ExitRule()
        {
            var fragPos = this.CurrentNodes.First().Fragment.Position;
            var fragLen = this.CurrentNodes.Last().Fragment.Position + this.CurrentNodes.Last().Fragment.Length - fragPos;
            var fragment = new StringFragment(this.Text, fragPos, fragLen);

            // Console.WriteLine("exit " + this.CurrentRule.Name + " @" + this.Position);
            return new ParsingState(
                this,
                this.Parent.CurrentRule,
                this.Parent.Parent,
                this.Position,
                this.Parent.Skipping,
                this.Parent.CurrentNodes.Concat(new[] {
                    new StringTreeNode(fragment, this.CurrentRule, this.CurrentNodes.ToArray())
                })
            ) { Depth = this.Depth - 1 };
        }
    }
}
