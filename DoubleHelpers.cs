using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CompareInliningBenchmarks;

public struct StandardDoubleComparer
    : IComparer<double>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(double x, double y)
    {
        return x.CompareTo(y);
    }
}

public struct FullyInlinedDoubleComparer
    : IComparer<double>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(double x, double y)
    {
        return DoubleHelpers.CompareFullyInlined(x, y);
    }
}

public struct SplitInlinedDoubleComparer
    : IComparer<double>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(double x, double y)
    {
        return DoubleHelpers.CompareSplitInlined(x, y);
    }
}

public static class DoubleHelpers
{
    // This is a copy of Double.Compare()
    public static int CompareStandard(double x, double y)
    {
        if (x < y)
        {
            return -1;
        }
        if (x > y)
        {
            return 1;
        }
        if (x == y)
        {
            return 0;
        }
        if (!double.IsNaN(x))
        {
            return 1;
        }
        if (!double.IsNaN(y))
        {
            return -1;
        }
        return 0;
    }

    // This is a copy of Double.Compare() but w/ AggressiveInlining
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareFullyInlined(double x, double y)
    {
        if (x < y)
        {
            return -1;
        }
        if (x > y)
        {
            return 1;
        }
        if (x == y)
        {
            return 0;
        }
        if (!Double.IsNaN(x))
        {
            return 1;
        }
        if (!Double.IsNaN(y))
        {
            return -1;
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareSplitInlined(double x, double y)
    {
        if (x < y)
        {
            return -1;
        }
        if (x > y)
        {
            return 1;
        }
        if (x == y)
        {
            return 0;
        }
        return CompareWithAtLeastOneNaN(x, y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int CompareWithAtLeastOneNaN(double x, double y)
    {
        if (!double.IsNaN(x))
        {
            return 1;
        }
        if (!double.IsNaN(y))
        {
            return -1;
        }
        return 0;
    }
}
