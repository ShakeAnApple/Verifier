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
            //            var testText = @"
            //never { /* !( ([]<> !np_) -> (<> q) ) */
            //T0_init:
            //        if
            //        :: (!q) -> goto T0_init
            //        :: (!q && !np_) -> goto accept_S1
            //        fi;
            //accept_S1:
            //        if
            //        :: (!q) -> goto T0_init
            //        :: (!q && !np_) -> goto accept_S1
            //        fi;
            //}
            //";
            string ltl = "[] (<> power_on)";
            //string ltl = "[](machine_type -> x [](machine_type -> x namewitharrow))";
            //var p = new LtlParser.LtlAutomatonTextParser();
            var ltlAutomaton = LtlParser.Parse(ltl);
            ltlAutomaton.SaveAsDgmlGraph(@"c:\tmp\l.dgml");

            var filepath = args.Length != 0 ? args[0] : string.Empty;
            //while (filepath == string.Empty || filepath.ToLower() == "exit")
            //{
            //    Console.WriteLine("Input file path");
            //    filepath = Console.ReadLine();
            //}

            filepath = @"C:\Projects\Uni\Verification\test.xstd";

            Automaton sm = null;
            try
            {
                var sz = new ChartXmlSerializer();
                sm = sz.LoadFromXml(filepath);
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

            var modelAutomaton = sm.ToTlaAutomaton(true);
            modelAutomaton.SaveAsDgmlGraph(@"c:\tmp\m.dgml");

            var v = new AutomatonVerifier(modelAutomaton);
            var result = v.Verify(ltlAutomaton);

            Console.WriteLine(result == null ? "<NULL>" : result.Count.ToString());

            if (result != null)
            {
                Console.WriteLine("Example:");
                foreach (var item in result)
                {
                    if (!item.Tag.ToString().Contains('|'))
                        Console.WriteLine("\t" + item.Tag);
                }
            }

            //var verifier = new Verifier(sm);
            //var ltl = "";
            //while (ltl != "exit")
            //{
            //    Console.WriteLine("Input LTL:");

            //    //ltl = Console.ReadLine();
            //    ltl = "[](!p -> <>(z U (x || a)))";
            //    var ltlBuchiAutomata = LtlParser.Parse(ltl);

            //    var result = verifier.Verify(ltlBuchiAutomata);
            //    if (result.Count != 0)
            //    {
            //        Console.WriteLine("specification {0} is FALSE \n see LTL counterexample: ", ltl);
            //        foreach (var counterexample in result)
            //        {
            //            Console.WriteLine(counterexample);
            //        }    
            //    }
            //    else
            //    {
            //        Console.WriteLine("specification {0} is TRUE", ltl);
            //    }
            //}
        }
    }
}
