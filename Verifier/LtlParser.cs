﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Verifier.Model;
using Verifier.Tla;
using Verifier.LtlAutomatonParser;
//using rwth.i2.ltl2ba4j.formula.impl;
//using rwth.i2.ltl2ba4j.model;
//using rwth.i2.ltl2ba4j;

namespace Verifier
{
    public class LtlParser : IDisposable
    {
        readonly DirectoryInfo _tempDir;
        readonly string _ltl2baExecutablePath;

        public LtlParser()
        {
            var userTempFolderPath = Path.GetTempPath();
            string tempFolderPath;

            do { tempFolderPath = Path.Combine(userTempFolderPath, Guid.NewGuid().ToString()); }
            while (Directory.Exists(tempFolderPath));

            _tempDir = Directory.CreateDirectory(tempFolderPath);

            if (RunningPlatform() == Platform.Windows)
            {
                _ltl2baExecutablePath = Path.Combine(_tempDir.FullName, "ltl2ba.exe");
                File.WriteAllBytes(_ltl2baExecutablePath, LtlParserResources.ltl2ba_win32);
            }
            else
            {
                _ltl2baExecutablePath = Path.Combine(_tempDir.FullName, "ltl2ba");
                File.WriteAllBytes(_ltl2baExecutablePath, LtlParserResources.ltl2ba_lin32);
            }
        }

        public TlaAutomaton Parse(string ltl)
        {
            var parser = new LtlParser();

            var strAutomaton = parser.GetStringAutomaton(parser.NegateLtl(ltl));
            var automaton = parser.ParseStringAutomaton(strAutomaton);

            return automaton;
        }

        private string GetStringAutomaton(string ltl)
        {
            var processStartInfo = new ProcessStartInfo(_ltl2baExecutablePath) {
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
            var automaton = automatonParser.ParseTlaAutomaton(strAutomaton, true);

            return automaton;
        }

        private string NegateLtl(string ltl)
        {
            return $"!({ltl})";
        }

        public void Dispose()
        {
            try { _tempDir.Delete(true); }
            catch
            {
                Thread.Sleep(500);
                try { _tempDir.Delete(true); }
                catch { }
            }
        }

        public enum Platform
        {
            Windows,
            Linux,
            Mac
        }

        public static Platform RunningPlatform()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
                    // Instead of platform check, we'll do a feature checks (Mac specific root folders)
                    if (Directory.Exists("/Applications")
                        & Directory.Exists("/System")
                        & Directory.Exists("/Users")
                        & Directory.Exists("/Volumes"))
                        return Platform.Mac;
                    else
                        return Platform.Linux;

                case PlatformID.MacOSX:
                    return Platform.Mac;

                default:
                    return Platform.Windows;
            }
        }
    }
}
