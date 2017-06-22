using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg.Parser;

namespace PolinaCompiler.Peg.Model
{
    public class GrammarModelSourceGenerator
    {
        public string Namespace { get; set; }
        public bool Public { get; set; }

        public GrammarModelSourceGenerator()
        {
        }

        public IndentedWriter Generate(ParsingGrammar g)
        {
            var mb = new ModelBuilder(g);
            var model = mb.Complete();
            model.Namespace = string.IsNullOrWhiteSpace(this.Namespace) ? g.Name : this.Namespace;
            model.Public = this.Public;

            var gen = new ModelSourceCodeGenerator(model);
            return gen.Generate();
        }
    }
}
