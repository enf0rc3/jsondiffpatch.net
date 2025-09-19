using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JsonDiffPatchDotNet
{
    internal class Lcs
    {
        internal List<JToken> Sequence { get; set; }

        internal List<int> Indices1 { get; set; }

        internal List<int> Indices2 { get; set; }

        private Lcs()
        {
            Sequence = new List<JToken>();
            Indices1 = new List<int>();
            Indices2 = new List<int>();
        }

        internal static Lcs Get(List<JToken> left, List<JToken> right, ItemMatch match, bool matchByPosition = true)
        {
            var matrix = LcsInternal(left, right, match, matchByPosition);
            var result = Backtrack(matrix, left, right, left.Count, right.Count, match, matchByPosition);
            return result;
        }

        private static int[,] LcsInternal(List<JToken> left, List<JToken> right, ItemMatch match, bool matchByPosition)
        {
            var arr = new int[left.Count + 1, right.Count + 1];

            for (int i = 1; i <= left.Count; i++)
            {
                for (int j = 1; j <= right.Count; j++)
                {
                    if (match.MatchArrayElement(left[i - 1], i-1, right[j - 1], j-1, matchByPosition))
                    {
                        arr[i, j] = arr[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        arr[i, j] = Math.Max(arr[i - 1, j], arr[i, j - 1]);
                    }
                }
            }

            return arr;
        }

        private static Lcs Backtrack(int[,] matrix, List<JToken> left, List<JToken> right, int li, int ri, ItemMatch match, bool matchByPosition)
        {
            var index1 = li;
            var index2 = ri;
            var result = new Lcs();

            while (index1 != 0 && index2 != 0)
            {
                var sameLetter = match.MatchArrayElement(left[index1 - 1], index1 - 1, right[index2 - 1], index2 - 1, matchByPosition);

                if (sameLetter)
                {
                    result.Sequence.Add(left[index1 - 1]);
                    result.Indices1.Add(index1 - 1);
                    result.Indices2.Add(index2 - 1);
                    --index1;
                    --index2;
                }
                else
                {
                    var valueAtMatrixAbove = matrix[index1, index2 - 1];
                    var valueAtMatrixLeft = matrix[index1 - 1, index2];
                    if (valueAtMatrixAbove > valueAtMatrixLeft)
                    {
                        --index2;
                    }
                    else
                    {
                        --index1;
                    }
                }
            }

            return result;
        }
    }
}
