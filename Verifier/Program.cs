using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PolinaCompiler.Peg;
using Verifier.Model;
using Verifier.Tla;
using Verifier.Xml;

namespace Verifier
{

    class Program : IDisposable
    {
        class CommandLineArguments
        {
            [ArgAlias("m")]
            [ArgDescription("File with automaton for verification, otherwise interactive mode")]
            public string ModelAutomatonFile { get; set; }

            [ArgAlias("f")]
            [ArgDescription("Ltl formula to verificate on model (if loaded), or interactive mode")]
            public string LtlFormula { get; set; }

            [ArgAlias("b")]
            [ArgDescription("Run commands or Ltl formulas sequence from specified file")]
            public string BatchTest { get; set; }

            [ArgAlias("md")]
            [ArgDescription("Save model (if loaded) automaton diagram to dgml file")]
            public string ModelDiagram { get; set; }

            [ArgAlias("fd")]
            [ArgDescription("Save Ltl formula (if presented) automaton diagram to dgml file")]
            public string FormulaDiagram { get; set; }

            [ArgAlias("vd")]
            [ArgDescription("Save virifier automaton (m and f intersection) diagram to dgml file")]
            public string VerifierDiagram { get; set; }

            [ArgAlias("h")]
            [ArgDescription("Show help (or just 'help' in interactive mode)")]
            public bool Help { get; set; }
        }

        const string _helpMessage = @"
Available commands:
    help                         - show this help
    load <model.xstd>            - load model automaton for verification
    run  <ltlexprs.txt>          - run commands or Ltl formulas sequence from specified file
    save model <model.dgml>      - save last model automaton diagram to dgml file
    save formula <formula.dgml>  - save last Ltl formula automaton diagram to dgml file
    save verifier <verfier.dgml> - save last virifier automaton diagram to dgml file
    exit                         - exit from interactive mode
";


        readonly LtlParser _ltlParser = new LtlParser();
        readonly ChartXmlSerializer _modelLoader = new ChartXmlSerializer();

        AutomatonVerifier _verifier = null;
        TlaAutomaton _model = null;
        TlaAutomaton _ltlFormula = null;

        public Program()
        {
        }

        private void DoInteractive()
        {
            Console.WriteLine("Input LTL-formula or command (help for commands list or exit):");

            var ltlOrCmd = this.ReadCmdLine();
            while (ltlOrCmd != "exit")
            {
                this.PerformCommand(ltlOrCmd);

                ltlOrCmd = this.ReadCmdLine();
            }
        }

        void PerformCommand(string ltlOrCmd)
        {
            try
            {
                this.ExecuteCommand(ltlOrCmd);
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                    throw;

                Console.WriteLine(ex.Message);
            }
        }

        void ExecuteCommand(string ltlOrCmd)
        {
            var cmdParts = ltlOrCmd.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            switch (cmdParts.First().ToLower())
            {
                case "help":
                    {
                        Console.WriteLine(_helpMessage);
                    }
                    break;
                case "load":
                    {
                        if (cmdParts.Length < 2)
                            Console.WriteLine("File name required!");
                        else
                            this.LoadModelAutomaton(cmdParts.Last());
                    }
                    break;
                case "run":
                    {
                        if (cmdParts.Length < 2)
                            Console.WriteLine("File name required!");
                        else
                            this.RunCommandsFromFile(cmdParts.Last());
                    }
                    break;
                case "save":
                    {
                        var saveCmdParts = cmdParts.Last().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (saveCmdParts.Length < 2)
                        {
                            Console.WriteLine("File name required!");
                        }
                        else
                        {
                            switch (saveCmdParts.First().ToLower())
                            {
                                case "model": this.SaveModelDiagram(saveCmdParts.Last()); break;
                                case "formula": this.SaveLtlDiagram(saveCmdParts.Last()); break;
                                case "verifier": this.SaveVerifierDiagram(saveCmdParts.Last()); break;
                                default:
                                    Console.WriteLine("Unknown command.");
                                    break;
                            }
                        }
                    }
                    break;
                default: // otherwise treat it as ltl-formula
                    this.Verify(ltlOrCmd);
                    break;
            }

        }

        string ReadCmdLine()
        {
            string line;

            do
            {
                Console.Write("> ");
                line = Console.ReadLine();
            }
            while (string.IsNullOrWhiteSpace(line));

            return line;
        }

        void LoadModelAutomaton(string fileName)
        {
            try
            {
                _model = _modelLoader.LoadFromXml(fileName);
                _verifier = new AutomatonVerifier(_model);
                Console.WriteLine("Automaton loaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (_model == null)
                Console.WriteLine("Can't create automaton");
        }

        void SaveModelDiagram(string diagramFileName)
        {
            if (_model != null)
                _model.SaveAsDgmlGraph(diagramFileName);
            else
                Console.WriteLine("Nothing to save: model automaton is not loaded.");
        }

        void Verify(string ltl)
        {
            _ltlFormula = _ltlParser.Parse(ltl);

            if (_verifier != null)
            {
                var result = _verifier.Verify(_ltlFormula);
                if (result != null)
                {
                    Console.WriteLine("Specification {0} is FALSE", ltl);
                    Console.WriteLine("\tsee LTL counterexample: ");
                    foreach (var counterexample in result)
                    {
                        if (!counterexample.Tag.ToString().Contains('|'))
                            Console.WriteLine("\t\t" + counterexample.Tag);
                    }
                }
                else
                {
                    Console.WriteLine("Specification {0} is TRUE", ltl);
                }

            }
            else
            {
                Console.WriteLine("Verifier is not initialized with model.");
            }
        }

        void SaveLtlDiagram(string diagramFileName)
        {
            if (_ltlFormula != null)
                _ltlFormula.SaveAsDgmlGraph(diagramFileName);
            else
                Console.WriteLine("Nothing to save: no ltl formula recognized.");
        }

        void SaveVerifierDiagram(string diagramFileName)
        {
            if (_verifier != null && _verifier.LastVerificationGraphInfo != null)
                _verifier.LastVerificationGraphInfo.MakeXmlDocument().Save(diagramFileName);
            else
                Console.WriteLine("Nothing to save: no verification automaton holded currently.");
        }

        void RunCommandsFromFile(string fileName)
        {
            using (var reader = File.OpenText(fileName))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine().Trim();

                    if (!line.StartsWith("#") && !line.StartsWith("//"))
                        this.PerformCommand(line);
                }
            }
        }

        public void Dispose()
        {
            _ltlParser.Dispose();
        }

        static void Main(string[] args)
        {
            var options = new CommandLineArguments();
            var argsParser = new CommandLineAnalyzer<CommandLineArguments>();

            if (argsParser.TryParse(args, options) && !options.Help)
            {
                using (var app = new Program())
                {
                    if (!string.IsNullOrWhiteSpace(options.ModelAutomatonFile))
                    {
                        app.LoadModelAutomaton(options.ModelAutomatonFile);

                        if (!string.IsNullOrWhiteSpace(options.ModelDiagram))
                            app.SaveModelDiagram(options.ModelDiagram);
                    }

                    if (!string.IsNullOrWhiteSpace(options.BatchTest))
                    {
                        app.RunCommandsFromFile(options.BatchTest);
                    }
                    else if (string.IsNullOrWhiteSpace(options.LtlFormula))
                    {
                        app.DoInteractive();
                    }
                    else
                    {
                        app.Verify(options.LtlFormula);

                        if (!string.IsNullOrWhiteSpace(options.FormulaDiagram))
                            app.SaveLtlDiagram(options.FormulaDiagram);

                        if (!string.IsNullOrWhiteSpace(options.VerifierDiagram))
                            app.SaveVerifierDiagram(options.VerifierDiagram);
                    }
                }
            }
            else
            {
                Console.WriteLine(argsParser.MakeHelp());
            }
        }
    }
}