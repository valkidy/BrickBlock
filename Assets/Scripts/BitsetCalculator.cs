using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Helper;

public class BitsetCalculator
{
    static public void CalculateTotalBitsets()
    {
        var records = new List<int>();
        var SetsOfRotate = new List<int[]>();

        for (int i = 1; i <= 255; ++i)
        {
            int[] subset = new int[4];
            BitBlock b = new BitBlock(i);
            for (int j = 0; j < 4; ++j)
            {
                int bitValue = b.IntValue;
                if (-1 == records.IndexOf(bitValue))
                {
                    records.Add(bitValue);
                    subset[j] = bitValue;
                }
                b.Rotate();
            }

            if (subset.Sum() > 0)
            {
                SetsOfRotate.Add(subset);
            }
        }

        string formatString = string.Empty;
        foreach (var set in SetsOfRotate)
        {
            formatString += $"{set[0]}, {set[1]}, {set[2]}, {set[3]}\n";
        }
        Debug.Log($"Total sets : {SetsOfRotate.Count} \n{formatString}");

        records.Clear();

        var SetsOfMirror = new List<(int, int)[]>();
        var group = Enumerable
                        .Repeat(1, SetsOfRotate.Count)
                        .Select((i, j) => SetsOfRotate[j][0])
                        .ToList();

        for (int i = 0; i < group.Count; ++i)
        {
            (int, int)[] subset = new (int, int)[3];
            for (int j = 0; j <= 2; ++j)
            {
                int bit = group[i];

                BitBlock b = new BitBlock(bit);
                b.Mirror(j - 1);

                int bitValue = b.IntValue;
                if (j > 0)
                {
                    int baseIndex = SetsOfRotate.FindIndex(x => Array.IndexOf(x, subset[0].Item1) != -1);
                    int nextIndex = SetsOfRotate.FindIndex(x => Array.IndexOf(x, bitValue) != -1);
                    if (baseIndex != nextIndex && !records.Contains(bitValue))
                    {
                        records.Add(bitValue);
                        subset[j] = (SetsOfRotate[nextIndex][0], Array.IndexOf(SetsOfRotate[nextIndex], bitValue));
                    }
                }

                if (j == 0 && !records.Contains(bitValue))
                {
                    records.Add(bitValue);
                    subset[j] = (bitValue, 0);
                }
            }

            if (subset.Count(item => item.Item1 > 0) > 1)
            {
                SetsOfMirror.Add(subset);
            }
        }

        formatString = string.Empty;
        foreach (var set in SetsOfMirror)
        {
            formatString += $"{set[0].Item1}, {set[1].Item1}({set[1].Item2}), {set[2].Item1}({set[2].Item2})\n";
        }
        Debug.Log($"Total sets : {SetsOfMirror.Count} \n{formatString}");
    }
}
