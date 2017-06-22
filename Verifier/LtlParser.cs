using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verifier.Model;
using Verifier.Tla;
using Verifier.LtlAutomatonParser;
//using rwth.i2.ltl2ba4j.formula.impl;
//using rwth.i2.ltl2ba4j.model;
//using rwth.i2.ltl2ba4j;

namespace Verifier
{
    public class LtlParser
    {
        private LtlParser() {}

        public static TlaAutomaton Parse(string ltl)
        {
            var parser = new LtlParser();

            var strAutomaton = parser.GetStringAutomaton(parser.NegateLtl(ltl));
            var automaton = parser.ParseStringAutomaton(strAutomaton);

            return automaton;
        }

        private string GetStringAutomaton(string ltl)
        {
            var processStartInfo = new ProcessStartInfo("ltl2ba.exe") {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = string.Format("-f \"{0}\"", ltl)
            };

            var process = Process.Start(processStartInfo);
            process.WaitForExit();

            var automatonStr = "";
            using (var output = process.StandardOutput)
            {
                automatonStr = output.ReadToEnd();
            }

            return automatonStr;
        }

        private TlaAutomaton ParseStringAutomaton(string strAutomaton)
        {
            var automatonParser = new LtlAutomatonTextParser();
            var automaton = automatonParser.TryParseTlaAutomaton(strAutomaton, true);

            return automaton;
        }

        private string NegateLtl(string ltl)
        {
            return $"!({ltl})";
        }
        
    }
}
