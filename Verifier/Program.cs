using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verifier.Model;
using Verifier.Tla;
using Verifier.Xml;


namespace Verifier
{
    class Program
    {
        static void Main(string[] args)
        {
            var filepath = args.Length != 0 ? args[0] : string.Empty;

            TlaAutomaton sm = null;
            try
            {
                var sz = new ChartXmlSerializer();
                var a = sz.LoadFromXml(filepath);
                sm = a.ToTlaAutomaton(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            if (sm == null)
            {
                Console.WriteLine("Can't create automaton");
                return;
            }

            sm.SaveAsDgmlGraph(@"v:\m.dgml");

            using (var ltlParser = new LtlParser())
            {
                var verifier = new AutomatonVerifier(sm);
                var ltl = Console.ReadLine();
                while (ltl != "exit")
                {
                    Console.WriteLine("Input LTL:");

                    try
                    {
                        var ltlBuchiAutomata = ltlParser.Parse(ltl);
                        ltlBuchiAutomata.SaveAsDgmlGraph(@"v:\l.dgml");

                        var result = verifier.Verify(ltlBuchiAutomata);
                        if (result != null)
                        {
                            Console.WriteLine("specification {0} is FALSE \n see LTL counterexample: ", ltl);
                            foreach (var counterexample in result)
                            {
                                if (!counterexample.Tag.ToString().Contains('|'))
                                    Console.WriteLine(counterexample.Tag);
                            }
                        }
                        else
                        {
                            Console.WriteLine("specification {0} is TRUE", ltl);
                        }
                    }
                    catch (ApplicationException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                    do { ltl = Console.ReadLine(); }
                    while (string.IsNullOrWhiteSpace(ltl));
                }
            }
        }
    }
}