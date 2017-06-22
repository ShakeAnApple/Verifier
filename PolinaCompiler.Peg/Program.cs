using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PolinaCompiler.Peg.Def;
using PolinaCompiler.Peg.Model;
using PolinaCompiler.Peg.Parser;
using PolinaCompiler.Peg.Xml;

namespace PolinaCompiler.Peg
{
    static class Program
    {
        class CommandLineArguments
        {
            [DefaultArg(0)]
            [ArgDescription("File with grammar to generate model for")]
            public string GrammarFile { get; set; }

            [DefaultArg(1)]
            [ArgDescription("File for generated model and mapping")]
            public string ModelFile { get; set; }

            [ArgAlias("s")]
            [ArgDescription("Grammar start rule name")]
            public string StartRule { get; set; }
            
            [ArgAlias("n")]
            [ArgDescription("Namespace for generated model")]
            public string Namespace { get; set; }

            [ArgAlias("x")]
            [ArgDescription("Input grammar is in Xml form")]
            public bool XmlGrammar { get; set; }

            [ArgAlias("p")]
            [ArgDescription("Generate model with public visibility")]
            public bool Public { get; set; }

            [ArgAlias("h")]
            [ArgDescription("Show help")]
            public bool Help { get; set; }
        }

        static void Main(string[] args)
        {
            var options = new CommandLineArguments();
            var argsParser = new CommandLineAnalyzer<CommandLineArguments>();

            if (argsParser.TryParse(args, options) && !options.Help)
            {
                ParsingGrammar grammar;

                if (options.XmlGrammar)
                {
                    var loader = new GrammarXmlSerializer();
                    grammar = loader.LoadFromFile(options.GrammarFile);
                }
                else
                {
                    grammar = DefenitionsGrammar.TryParse(File.ReadAllText(options.GrammarFile));
                }

                if (!string.IsNullOrWhiteSpace(options.StartRule))
                    grammar.StartRuleName = options.StartRule;

                var sg = new GrammarModelSourceGenerator();

                if (!string.IsNullOrWhiteSpace(options.Namespace))
                    sg.Namespace = options.Namespace;

                sg.Public = options.Public;

                var generatedText = sg.Generate(grammar);

                File.WriteAllText(options.ModelFile, generatedText.GetContentAsString());
            }
            else
            {
                Console.WriteLine(argsParser.MakeHelp());
            }
        }
    }
}
