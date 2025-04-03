using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaultLocalizationNN
{
    internal static class NearestNeighbourFaultLocalizer
    {

        // Fault localization using binary coverage spectra, Hamming's distance and set difference
        private static Dictionary<int, string>[] LocalizeFaultsBinaryCoverageSpectra(LineInfo[][] traces, string[] testOutputs, string[] correctOutputs)
        {

            // Input data validation
            if (
                traces == null ||
                traces.Contains(null) ||
                traces.Any(trace => trace.Contains(null)) ||
                testOutputs == null ||
                testOutputs.Contains(null) ||
                correctOutputs == null ||
                correctOutputs.Contains(null) ||
                traces.Any(trace => trace.Length != traces[0].Length)
            )
                return null;

            // Retrieve a spectrum from a respective trace and classify each spectrum as successful or failing 
            List<bool[]> successfulSpectra = new();
            List<bool[]> failingSpectra = new();
            for (int i = 0; i < traces.Length; i++)
            {

                bool[] spectrum = new bool[traces[0].Length];
                for (int j = 0; j < spectrum.Length; j++)
                    spectrum[j] = traces[i][j].Executions > 0;

                if (testOutputs[i] == correctOutputs[i])
                    successfulSpectra.Add(spectrum);
                else
                    failingSpectra.Add(spectrum);

            }

            List<Dictionary<int, string>> reports = new();
            foreach (bool[] failingSpectrum in failingSpectra)
            {

                // Find the index of the successful spectrum closest to the failing spectrum
                int minDistance = int.MaxValue;
                int nearestNeighbourIndex = 0;
                bool isMinDistanceZero = false;
                for (int i = 0; i < successfulSpectra.Count; i++)
                {

                    int distance = 0;
                    for (int j = 0; j < successfulSpectra[0].Length; j++)
                    {
                        if (successfulSpectra[i][j] ^ failingSpectrum[j])
                            distance++;
                    }

                    if (distance < minDistance)
                    {
                        if (distance == 0)
                        {
                            isMinDistanceZero = true;
                            break;
                        }
                        minDistance = distance;
                        nearestNeighbourIndex = i;
                    }

                }
                if (isMinDistanceZero)
                    continue;

                bool[] nearestNeighbourSpectrum = successfulSpectra[nearestNeighbourIndex];

                // Compute the difference between the nearest neighbour and the failing spectrum and map it to the source code lines
                LineInfo[] arbitraryTrace = traces[0];
                Dictionary<int, string> report = arbitraryTrace
                    .Select(line => new { line.Number, line.Code })
                    .Where((line, i) => failingSpectrum[i] && !nearestNeighbourSpectrum[i])
                    .OrderBy(line => line.Number)
                    .ToDictionary(line => line.Number, line => line.Code);

                // Add report to the list if it is not empty and doesn't duplicate any other report
                if (
                    report.Count != 0 &&
                    !reports.Any(r => r
                        .Select(line => line.Key)
                        .Except(report.Select(line => line.Key))
                        .Count() == 0
                    )
                )
                    reports.Add(report);

            }

            return reports.ToArray();

        }

        // Get the longest common subsequence indices for two sequences of integer numbers
        private static (int, int)[] GetLcsInt(int[] A, int[] B)
        {


            if (A == null || B == null || A.Length != B.Length)
                return null;

            if (A.Length == 0 || B.Length == 0)
                return new (int, int)[0];

            int n = A.Length;
            A = A.Prepend(-1).ToArray();
            B = B.Prepend(-1).ToArray();
            int[] thresh = new int[n + 1];
            List<int>[] matchlist = new List<int>[n + 1];
            List<(int indexA, int indexB)>[] link = new List<(int indexA, int indexB)>[n + 1];

            for (int i = 1; i < n + 1; i++)
            {
                matchlist[i] = new List<int>();
                for (int j = n; j >= 1; j--)
                {
                    if (A[i] == B[j])
                        matchlist[i].Add(j);
                }
            }

            thresh[0] = 0;
            for (int i = 1; i < n + 1; i++)
            {
                thresh[i] = n + 1;
            }
            link[0] = new List<(int indexA, int indexB)> { (-1, -1) };

            for (int i = 1; i < n + 1; i++)
            {
                foreach (int j in matchlist[i])
                {

                    int low = 1;
                    int high = n;
                    while (low < high)
                    {
                        int middle = (low + high) / 2;
                        if (thresh[middle] < j)
                            low = middle + 1;
                        else
                            high = middle;
                    }
                    int k = low;

                    if (j < thresh[k])
                    {
                        thresh[k] = j;
                        link[k] = new List<(int indexA, int indexB)> { (i, j) };
                        link[k].AddRange(link[k - 1]);
                    }

                }
            }

            int largestK = n;
            for (int i = 1; i < thresh.Length; i++)
            {
                if (thresh[i] == n + 1)
                {
                    largestK = i - 1;
                    break;
                }
            }

            return link[largestK]
                .Take(largestK)
                .Select(pair => (pair.indexA - 1, pair.indexB - 1))
                .Reverse()
                .ToArray();

        }

        // Fault localization using permutation spectra, Ulam's distance and moved elements difference
        private static Dictionary<int, string>[] LocalizeFaultsPermutationSpectra(LineInfo[][] traces, string[] testOutputs, string[] correctOutputs)
        {

            // Input data validation
            if (
                traces == null ||
                traces.Contains(null) ||
                traces.Any(trace => trace.Contains(null)) ||
                testOutputs == null ||
                testOutputs.Contains(null) ||
                correctOutputs == null ||
                correctOutputs.Contains(null) ||
                traces.Any(trace => trace.Length != traces[0].Length)
            )
                return null;

            // Retrieve a spectrum from a respective trace and classify each spectrum as successful or failing 
            List<int[]> successfulSpectra = new();
            List<int[]> failingSpectra = new();
            for (int i = 0; i < traces.Length; i++)
            {

                int[] spectrum = traces[i]
                    .Select(line => new { line.Executions, line.Number })
                    .GroupBy(
                        line => line.Executions,
                        line => line.Number,
                        (key, group) => new { Key = key, Group = group.OrderBy(number => number) }
                    )
                    .OrderBy(pair => pair.Key)
                    .SelectMany(pair => pair.Group)
                    .ToArray();

                if (testOutputs[i] == correctOutputs[i])
                    successfulSpectra.Add(spectrum);
                else
                    failingSpectra.Add(spectrum);

            }

            List<Dictionary<int, string>> reports = new();
            foreach (int[] failingSpectrum in failingSpectra)
            {

                // Find the index of the successful spectrum closest to the failing spectrum
                int minDistance = int.MaxValue;
                (int, int)[] nearestNeighbourLcsIndices = null; // Longest common subsequence (LCS) indices of the failing spectrum and the nearest neighbour spectrum
                bool isMinDistanceZero = false;
                for (int i = 0; i < successfulSpectra.Count; i++)
                {

                    (int, int)[] lcsIndices = GetLcsInt(failingSpectrum, successfulSpectra[i]);
                    if (lcsIndices == null)
                        return null;

                    int distance = failingSpectrum.Length - lcsIndices.Length;
                    if (distance < minDistance)
                    {
                        if (distance == 0)
                        {
                            isMinDistanceZero = true;
                            break;
                        }
                        minDistance = distance;
                        nearestNeighbourLcsIndices = lcsIndices;
                    }

                }
                if (isMinDistanceZero)
                    continue;

                // Compute the difference between the nearest neighbour and the failing spectrum and map it to the source code lines
                int[] correctLineNumbers = new int[nearestNeighbourLcsIndices.Length];
                for (int i = 0; i < nearestNeighbourLcsIndices.Length; i++)
                {
                    int index = nearestNeighbourLcsIndices[i].Item1;
                    correctLineNumbers[i] = failingSpectrum[index];
                }
                LineInfo[] arbitraryTrace = traces[0];
                Dictionary<int, string> report = arbitraryTrace
                    .Where(line => !correctLineNumbers.Contains(line.Number))
                    .OrderBy(line => line.Number)
                    .ToDictionary(line => line.Number, line => line.Code);

                // Add report to the list if it is not empty and doesn't duplicate any other report
                if (
                    report.Count != 0 &&
                    !reports.Any(r => r
                        .Select(line => line.Key)
                        .Except(report.Select(line => line.Key))
                        .Count() == 0
                    )
                )
                    reports.Add(report);

            }

            return reports.ToArray();

        }

        // General fault localization with nearest neighbour queries
        public static Dictionary<int, string>[] LocalizeFaults(LineInfo[][] traces, string[] testOutputs, string[] correctOutputs, ProgramSpectrumType programSpectrum)
        {

            // Input data validation
            if (
                traces == null ||
                traces.Contains(null) ||
                traces.Any(trace => trace.Contains(null)) ||
                testOutputs == null ||
                testOutputs.Contains(null) ||
                correctOutputs == null ||
                correctOutputs.Contains(null) ||
                traces.Any(trace => trace.Length != traces[0].Length)
            )
                return null;

            // Choose method and localize faults
            Dictionary<int, string>[] reports = null;
            switch (programSpectrum)
            {
                case ProgramSpectrumType.BinaryCoverage:
                    {
                        reports = LocalizeFaultsBinaryCoverageSpectra(traces, testOutputs, correctOutputs);
                        break;
                    }
                case ProgramSpectrumType.Permutation:
                    {
                        reports = LocalizeFaultsPermutationSpectra(traces, testOutputs, correctOutputs);
                        break;
                    }
            }
            return reports;

        }

    }
}
