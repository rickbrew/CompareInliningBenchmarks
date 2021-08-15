using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CompareInliningBenchmarks;

public struct StandardFloatComparer
    : IComparer<float>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(float x, float y)
    {
        return x.CompareTo(y);
    }
}

public struct FullyInlinedFloatComparer
    : IComparer<float>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(float x, float y)
    {
        return FloatHelpers.CompareFullyInlined(x, y);
    }
}

public struct SplitInlinedFloatComparer
    : IComparer<float>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Compare(float x, float y)
    {
        return FloatHelpers.CompareSplitInlined(x, y);
    }
}

public static class FloatHelpers
{
    // This is a copy of float.Compare()
    public static int CompareStandard(float x, float y)
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
        if (!float.IsNaN(x))
        {
            return 1;
        }
        if (!float.IsNaN(y))
        {
            return -1;
        }
        return 0;
    }

    // This is a copy of float.Compare() but w/ AggressiveInlining
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareFullyInlined(float x, float y)
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
        if (!float.IsNaN(x))
        {
            return 1;
        }
        if (!float.IsNaN(y))
        {
            return -1;
        }
        return 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CompareSplitInlined(float x, float y)
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
    private static int CompareWithAtLeastOneNaN(float x, float y)
    {
        if (!float.IsNaN(x))
        {
            return 1;
        }
        if (!float.IsNaN(y))
        {
            return -1;
        }
        return 0;
    }
}
