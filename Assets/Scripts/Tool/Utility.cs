using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Helper
{
    public static class EnumerableHelper
    {
        public static IEnumerable<Vector3Int> ForEachIntEnumerable(int width, int height, int depth)
        {
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int k = 0; k < depth; k++)
                    {
                        yield return new Vector3Int(i, j, k);
                    }
                }
            }
        } // End of ForEachEnumerable

        public static IEnumerable<(T item, int index)> IndexOf<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (item, index));
        } // End of IndexOf
    }

    public static class ArrayHelper
    {
        public static void RotateLeft<T>(this T[] array, int index, int count)
        {
            T temp = array[index];
            Array.Copy(array, index + 1, array, index, count - 1);
            array[index + count - 1] = temp;
        }

        public static void RotateRight<T>(this T[] array, int index, int count)
        {
            T temp = array[index + count - 1];
            Array.Copy(array, index, array, index + 1, count - 1);
            array[index] = temp;
        }
    }

    public static class MathHelper
    {
        public static Vector3Int ToVector3Int(this Vector3 v)
            => new Vector3Int((int)v.x, (int)v.y, (int)v.z);

        public static void ZeroTransform(this GameObject self)        
            => self.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        public static int[] GetColumn(this int[,] self, int i)
        {
            int numOfCols = self.GetLength(1);

            int[] ret = new int[numOfCols];
            for (int j = 0; j < numOfCols; ++j)
            {
                ret[j] = self[i, j];
            }
            return ret;
        }
    }
}
