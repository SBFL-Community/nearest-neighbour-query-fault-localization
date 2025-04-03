using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FaultLocalizationNN
{
    internal class Utils
    {

        public static LineInfo[] GetTrace(string filePath)
        {

            List<LineInfo> gcovTrace = new();
            StreamReader gcovStreamReader = new(filePath);
            while (!gcovStreamReader.EndOfStream)
            {
                string[] gcovLineParts = gcovStreamReader.ReadLine().Split(':', 3);
                int number = Convert.ToInt32(Regex.Match(gcovLineParts[1], "[0-9]+").Value);
                if (number > 0)
                {
                    int executions = 0;
                    int.TryParse(Regex.Match(gcovLineParts[0], "[0-9]+").Value, out executions);
                    string code = gcovLineParts[2];
                    gcovTrace.Add(new LineInfo(executions, number, code));
                }
            }

            return gcovTrace.ToArray();

        }

        public static LineInfo[][] GetTraces(string[] tracePaths)
        {

            LineInfo[] firstRun = GetTrace(tracePaths[0]);
            LineInfo[][] gcovTraces = new LineInfo[tracePaths.Length][];
            gcovTraces[0] = firstRun;
            for (int i = 1; i < tracePaths.Length; i++)
                gcovTraces[i] = GetTrace(tracePaths[i]);

            return gcovTraces;

        }

        public static string[] GetOutputs(string[] outputPaths)
        {
            string[] outputs = new string[outputPaths.Length];
            for (int i = 0; i < outputPaths.Length; i++)
            {
                using (StreamReader streamReader = new(outputPaths[i]))
                {
                    outputs[i] = streamReader.ReadToEnd();
                }
            }
            return outputs;
        }

        private static (int, int)[] GetLcsStr(List<string> A, List<string> B)
        {

            if (A == null || B == null || A.Count != B.Count)
                return null;

            if (A.Count == 0 || B.Count == 0)
                return new (int, int)[0];

            int n = A.Count;
            A = A.Prepend("").ToList();
            B = B.Prepend("").ToList();
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

        public static Dictionary<int, string> GetDifferentStatements(string referenceSourcePath, string sampleSourcePath)
        {

            if (referenceSourcePath == null || sampleSourcePath == null)
                return null;

            List<string> referenceSource = new();
            List<string> sampleSource = new();
            using (
                StreamReader referenceStreamReader = new StreamReader(referenceSourcePath),
                sampleStreamReader = new StreamReader(sampleSourcePath)
            )
            {
                while (!referenceStreamReader.EndOfStream)
                    referenceSource.Add(referenceStreamReader.ReadLine());
                while (!sampleStreamReader.EndOfStream)
                    sampleSource.Add(sampleStreamReader.ReadLine());
            }

            int sourceLengthDifference = referenceSource.Count - sampleSource.Count;
            string[] alignmentLines = new string[Math.Abs(sourceLengthDifference)];
            for (int i = 0; i < alignmentLines.Length; i++)
            {
                alignmentLines[i] = "";
            }
            if (sourceLengthDifference > 0)
                sampleSource.AddRange(alignmentLines);
            else if (sourceLengthDifference < 0)
                referenceSource.AddRange(alignmentLines);
            (int, int)[] lcsIndices = GetLcsStr(referenceSource, sampleSource);
            Regex notStatementRegex = new Regex("(^ *//.*$|^ */[*][^*/]*$|^[^/*]*[*]/ *$|^ */[*].*[*]/ *$|^[^0-9A-Za-z]*$)");
            Dictionary<int, string> differentStatements = new();
            if (lcsIndices != null)
            {

                int[] sampleSourceLcsIndices = new int[lcsIndices.Length];

                for (int i = 0; i < sampleSourceLcsIndices.Length; i++)
                {
                    sampleSourceLcsIndices[i] = lcsIndices[i].Item2;
                }

                for (int i = 0; i < sampleSource.Count; i++)
                {
                    if (!sampleSourceLcsIndices.Contains(i) && !notStatementRegex.IsMatch(sampleSource[i]))
                        differentStatements.Add(i + 1, sampleSource[i]);
                }

            }

            return differentStatements;

        }

    }
}
