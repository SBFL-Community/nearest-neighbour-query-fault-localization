using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FaultLocalizationNN
{
    class Program
    {
        static void Main(string[] args)
        {

            ProgramSpectrumType programSpectrum = ProgramSpectrumType.BinaryCoverage;
            if (args.Length == 0)
            {
                Console.WriteLine(
                    "This program performs a fault localization with nearest neighbour queries on a program from the Siemens suite\n" +
                    "\n" +
                    "Usage:\n" +
                    "\tFaultLocalizationNN {-b|-p}\n" +
                    "\n" +
                    "Parameters:\n" +
                    "\t-b, --binary\tPerform fault localization using binary coverage spectra\n" +
                    "\t-p, --permutation\tPerform fault localization using permutation spectra\n"
                );
                return;
            }
            else if (args.Length > 1)
            {
                Console.WriteLine($"Too much parameters and arguments for the program: {args.Length}\n");
                return;
            }
            else
            {
                switch (args[0])
                {
                    case "-b" or "--binary": programSpectrum = ProgramSpectrumType.BinaryCoverage; break;
                    case "-p" or "--permutation": programSpectrum = ProgramSpectrumType.Permutation; break;
                    default:
                        {
                            Console.WriteLine($"The program doesn't have the parameter: {args[0]}\n");
                            return;
                        }
                }

            }

            const string siemensSuiteDirectory = "..\\..\\..\\..\\..\\..\\Siemens suite";
            const int programVersionsCount = 41;
            const int testCount = 1608;

            string originalOutputDirectory = $"{siemensSuiteDirectory}\\tcas\\outputs\\original";
            string[] originalOutputPaths = new string[testCount];
            for (int i = 0; i < originalOutputPaths.Length; i++)
                originalOutputPaths[i] = $"{originalOutputDirectory}\\t{i + 1}.txt";
            string[] originalOutputs = Utils.GetOutputs(originalOutputPaths);

            string programSpectrumStr = "binary coverage spectra";
            switch (programSpectrum)
            {
                case ProgramSpectrumType.BinaryCoverage: programSpectrumStr = "binary coverage spectra"; break;
                case ProgramSpectrumType.Permutation: programSpectrumStr = "permutation spectra"; break;
            }
            Console.WriteLine(
                $"Fault localization with nearest neighbour queries with {programSpectrumStr}\n" +
                "Program with faulty versions from the Siemens suite: tcas\n" +
                "\n" +
                "\n"
            );
            for (int i = 0; i < programVersionsCount; i++)
            {

                string tracesDirectory = $"{siemensSuiteDirectory}\\tcas\\traces\\versions\\v{i + 1}";
                string[] tracePaths = new string[testCount];
                for (int j = 0; j < testCount; j++)
                    tracePaths[j] = $"{tracesDirectory}\\t{j + 1}.gcov";
                LineInfo[][] traces = Utils.GetTraces(tracePaths);

                string versionOutputDirectory = $"{siemensSuiteDirectory}\\tcas\\outputs\\versions\\v{i + 1}";
                string[] versionOutputPaths = new string[testCount];
                for (int j = 0; j < versionOutputPaths.Length; j++)
                    versionOutputPaths[j] = $"{versionOutputDirectory}\\t{j + 1}.txt";
                string[] versionOutputs = Utils.GetOutputs(versionOutputPaths);

                Console.WriteLine($"Program version: {i + 1}\n");

                string originalSourcePath = $"{siemensSuiteDirectory}\\tcas\\original\\tcas.c";
                string versionSourcePath = $"{siemensSuiteDirectory}\\tcas\\versions\\v{i + 1}\\tcas.c";
                Dictionary<int, string> faultyLines = Utils.GetDifferentStatements(originalSourcePath, versionSourcePath);
                if (faultyLines != null && faultyLines.Count > 0)
                {
                    Console.WriteLine(
                        $"Actual faulty lines:\n" +
                        $" {"Number",-7} {"Code",-79}\n" +
                        $"+{new string('-', 7)}+{new string('-', 79)}+"
                     );
                    foreach (var line in faultyLines)
                    {
                        int lineNumber = line.Key;
                        string lineCode = line.Value.Trim();
                        lineCode = lineCode.Substring(0, Math.Min(lineCode.Length, 79));
                        Console.WriteLine($" {lineNumber,-7} {lineCode}");
                    }
                }
                else
                    Console.WriteLine("No faulty lines were found\n");

                Console.WriteLine();

                Dictionary<int, string>[] reports = NearestNeighbourFaultLocalizer.LocalizeFaults(traces, versionOutputs, originalOutputs, programSpectrum);
                if (reports != null && reports.Length > 0)
                {
                    for (int j = 0; j < reports.Length; j++)
                    {
                        Console.WriteLine(
                            $"Suspicious lines (report {j + 1}):\n" +
                            $" {"Number",-7} {"Code",-79}\n" +
                            $"+{new string('-', 7)}+{new string('-', 79)}+"
                         );
                        foreach (var line in reports[j])
                        {
                            int lineNumber = line.Key;
                            string lineCode = line.Value.Trim();
                            lineCode = lineCode.Substring(0, Math.Min(lineCode.Length, 79));
                            Console.WriteLine($" {lineNumber,-7} {lineCode}");
                        }
                        Console.WriteLine();
                    }
                }
                else
                    Console.WriteLine("No reports were produced\n");
                Console.WriteLine("\n");

            }

        }
    }
}