using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CompareInliningBenchmarks;

public static class ListUtil
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SwapElements<T, TList>(TList a, int i, int j)
        where TList : IList<T>
    {
        if (i != j)
        {
            T local = a[i];
            a[i] = a[j];
            a[j] = local;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FisherYatesShuffle<T>(IList<T> list, Random random)
    {
        FisherYatesShuffle<T>(list, 0, list.Count, random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FisherYatesShuffle<T>(IList<T> list, int startIndex, int length, Random random)
    {
        FisherYatesShuffle<T, IList<T>>(list, startIndex, length, random);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FisherYatesShuffle<T, TList>(TList list, Random random)
        where TList : IList<T>
    {
        FisherYatesShuffle<T, TList>(list, 0, list.Count, random);
    }

    public static void FisherYatesShuffle<T, TList>(TList list, int startIndex, int length, Random random)
        where TList : IList<T>
    {
        if (length == 0)
        {
            return;
        }

        // http://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle

        for (int i = (startIndex + length - 1); i >= (startIndex + 1); --i)
        {
            int j = startIndex + random.Next(i - startIndex + 1);
            SwapElements<T, TList>(list, i, j);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(IList<T> list)
    {
        Sort<T, IList<T>>(list);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T, TList>(TList list)
        where TList : IList<T>
    {
        Sort<T, TList, Comparer<T>>(list, 0, list.Count, Comparer<T>.Default);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(IList<T> list, IComparer<T> comparer)
    {
        Sort<T, IList<T>, IComparer<T>>(list, comparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T, TList, TComparer>(TList list, TComparer comparer)
        where TList : IList<T>
        where TComparer : IComparer<T>
    {
        Sort<T, TList, TComparer>(list, 0, list.Count, comparer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Sort<T>(IList<T> list, int startIndex, int length, IComparer<T> comparer)
    {
        Sort<T, IList<T>, IComparer<T>>(list, startIndex, length, comparer);
    }

    public static void Sort<T, TList, TComparer>(TList list, int startIndex, int length, TComparer comparer)
        where TList : IList<T>
        where TComparer : IComparer<T>
    {
        if (length == 0 || length == 1)
        {
            return;
        }

        SortImpl<T, TList, TComparer>(list, startIndex, length, comparer);
    }

    private static void SortImpl<T, TList, TComparer>(TList list, int startIndex, int length, TComparer comparer)
        where TList : IList<T>
        where TComparer : IComparer<T>
    {
        AlgorithmsWithComparer<T, TList, TComparer>.IntrospectiveSort(list, startIndex, length, comparer);
    }

    private static class AlgorithmsWithComparer<T, TList, TComparer>
        where TList : IList<T>
        where TComparer : IComparer<T>
    {
        public static void QuickSortImpl(TList keys, int left, int right, TComparer comparer)
        {
            do
            {
                int lo = left;
                int hi = right;
                int mid = lo + ((hi - lo) >> 1);

                SwapIfGreater(keys, comparer, lo, mid);
                SwapIfGreater(keys, comparer, lo, hi);
                SwapIfGreater(keys, comparer, mid, hi);

                T y = keys[mid];
                do
                {
                    while (comparer.Compare(keys[lo], y) < 0)
                    {
                        lo++;
                    }

                    while (comparer.Compare(y, keys[hi]) < 0)
                    {
                        hi--;
                    }

                    if (lo > hi)
                    {
                        break;
                    }

                    if (lo < hi)
                    {
                        T local2 = keys[lo];
                        keys[lo] = keys[hi];
                        keys[hi] = local2;
                    }

                    lo++;
                    hi--;
                } while (lo <= hi);

                if ((hi - left) <= (right - lo))
                {
                    if (left < hi)
                    {
                        QuickSortImpl(keys, left, hi, comparer);
                    }

                    left = lo;
                }
                else
                {
                    if (lo < right)
                    {
                        QuickSortImpl(keys, lo, right, comparer);
                    }

                    right = hi;
                }
            } while (left < right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Log2Floor(int x)
        {
            int num = 0;

            while (x >= 1)
            {
                num++;
                x >>= 1;
            }

            return num;
        }

        public static void IntrospectiveSort(TList keys, int startIndex, int length, TComparer comparer)
        {
            int depthLimit = 2 * Log2Floor(keys.Count);
            IntrospectiveSort(keys, startIndex, (startIndex + length) - 1, depthLimit, comparer);
        }

        public static void IntrospectiveSort(TList keys, int lo, int hi, int depthLimit, TComparer comparer)
        {
            while (hi > lo)
            {
                int num = (hi - lo) + 1;
                if (num <= 16)
                {
                    switch (num)
                    {
                        case 1:
                            return;

                        case 2:
                            SwapIfGreater(keys, comparer, lo, hi);
                            return;

                        case 3:
                            SwapIfGreater(keys, comparer, lo, hi - 1);
                            SwapIfGreater(keys, comparer, lo, hi);
                            SwapIfGreater(keys, comparer, hi - 1, hi);
                            return;

                        default:
                            InsertionSort(keys, lo, hi, comparer);
                            return;
                    }
                }

                if (depthLimit == 0)
                {
                    Heapsort(keys, lo, hi, comparer);
                    return;
                }

                depthLimit--;
                int num2 = PickPivotAndPartition(keys, lo, hi, comparer);
                IntrospectiveSort(keys, num2 + 1, hi, depthLimit, comparer);
                hi = num2 - 1;
            }
        }

        public static void Heapsort(TList keys, int lo, int hi, TComparer comparer)
        {
            int n = (hi - lo) + 1;

            for (int i = n >> 1; i >= 1; i--)
            {
                DownHeap(keys, i, n, lo, comparer);
            }

            for (int j = n; j > 1; j--)
            {
                SwapElements<T, TList>(keys, lo, (lo + j) - 1);
                DownHeap(keys, 1, j - 1, lo, comparer);
            }
        }

        public static void DownHeap(TList keys, int i, int n, int lo, TComparer comparer)
        {
            T x = keys[(lo + i) - 1];

            while (i <= (n >> 1))
            {
                int num = 2 * i;

                if ((num < n) && (comparer.Compare(keys[(lo + num) - 1], keys[lo + num]) < 0))
                {
                    num++;
                }

                if (comparer.Compare(x, keys[(lo + num) - 1]) >= 0)
                {
                    break;
                }

                keys[(lo + i) - 1] = keys[(lo + num) - 1];
                i = num;
            }

            keys[(lo + i) - 1] = x;
        }

        public static int PickPivotAndPartition(TList keys, int lo, int hi, TComparer comparer)
        {
            int b = lo + ((hi - lo) >> 1);

            SwapIfGreater(keys, comparer, lo, b);
            SwapIfGreater(keys, comparer, lo, hi);
            SwapIfGreater(keys, comparer, b, hi);

            T y = keys[b];
            SwapElements<T, TList>(keys, b, hi - 1);

            int i = lo;
            int j = hi - 1;

            while (i < j)
            {
                while (comparer.Compare(keys[++i], y) < 0)
                {
                }

                while (comparer.Compare(y, keys[--j]) < 0)
                {
                }

                if (i >= j)
                {
                    break;
                }

                SwapElements<T, TList>(keys, i, j);
            }

            SwapElements<T, TList>(keys, i, hi - 1);
            return i;
        }

        public static void InsertionSort(TList keys, int lo, int hi, TComparer comparer)
        {
            for (int i = lo; i < hi; i++)
            {
                int index = i;
                T x = keys[i + 1];

                while (index >= lo)
                {
                    T y = keys[index];
                    if (comparer.Compare(x, y) >= 0)
                    {
                        break;
                    }

                    keys[index + 1] = y;
                    --index;
                }

                keys[index + 1] = x;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SwapIfGreater(TList keys, TComparer comparer, int i, int j)
        {
            if (i != j)
            {
                T keyI = keys[i];
                T keyJ = keys[j];

                if (comparer.Compare(keyI, keyJ) > 0)
                {
                    keys[i] = keyJ;
                    keys[j] = keyI;
                }
            }
        }

        public static int InternalBinarySearch(TList array, int lowIndex, int length, T value, TComparer comparer)
        {
            int lo = lowIndex;
            int hi = (lowIndex + length) - 1;

            while (lo <= hi)
            {
                int mid = lo + ((hi - lo) >> 1);
                int comp = comparer.Compare(array[mid], value);

                if (comp == 0)
                {
                    return mid;
                }

                if (comp < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid - 1;
                }
            }

            return ~lo;
        }
    }
}
