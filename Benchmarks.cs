using BenchmarkDotNet.Attributes;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CompareInliningBenchmarks;

public unsafe class SortFloatsBenchmark
    : SortFloatingPointBenchmark<float, StandardFloatComparer, FullyInlinedFloatComparer, SplitInlinedFloatComparer>
{
    protected override int GetDataLength()
    {
        return 8 * 1000 * 1000;
    }

    protected override float FromDouble(double x)
    {
        return (float)x;
    }

    protected override float NaN => float.NaN;
}

public unsafe class SortDoublesBenchmark
    : SortFloatingPointBenchmark<double, StandardDoubleComparer, FullyInlinedDoubleComparer, SplitInlinedDoubleComparer>
{
    protected override int GetDataLength()
    {
        return 4 * 1000 * 1000;
    }

    protected override double FromDouble(double x)
    {
        return x;
    }

    protected override double NaN => float.NaN;
}

public unsafe abstract class SortFloatingPointBenchmark<T, TStandardComparer, TFullyInlinedComparer, TSplitInlinedComparer>
    where T : unmanaged
    where TStandardComparer : struct, IComparer<T>
    where TFullyInlinedComparer : struct, IComparer<T>
    where TSplitInlinedComparer : struct, IComparer<T>
{
    // Aim for a little less than 32MB, which equals Ryzen 5950X's 32MB of L3 cache per CCD
    protected abstract int GetDataLength();

    protected abstract T FromDouble(double x);

    protected abstract T NaN { get; }

    // Use native memory and an unchecked IList adapter to minimize overhead like bounds checking
    // and keep the spotlight on the Compare methods
    private int dataLength;
    private T* pData;
    private T* pBuffer;
    private NativeUncheckedArrayList<T> bufferAsList;

    // Don't use a percentage >50% because then the data is full of extents that are already sorted
    [Params(0, 10, 20, 30, 40, 50)]
    public int NaNPercentage;

    [GlobalSetup]
    public void Setup()
    {
        this.dataLength = GetDataLength();

        this.pData = (T*)NativeMemory.Alloc((nuint)this.dataLength, (nuint)sizeof(T));
        for (int i = 0; i < this.dataLength; ++i)
        {
            this.pData[i] = FromDouble(i);
        }

        if (NaNPercentage != 0)
        {
            // Evenly spread out some NaNs into the data
            int nanCount = (dataLength * NaNPercentage) / 100;
            for (int i = 0; i < nanCount; ++i)
            {
                this.pData[((long)i * (dataLength - 1)) / (nanCount - 1)] = this.NaN;
            }
        }

        // Shuffle things so that we aren't sorting data that's already sorted (well, unless NanPercentage is 100)
        Random random = new Random(0x729f16ee);
        NativeUncheckedArrayList<T> dataAsList = new NativeUncheckedArrayList<T>(this.pData, dataLength);
        ListUtil.FisherYatesShuffle<T, NativeUncheckedArrayList<T>>(dataAsList, random);

        this.pBuffer = (T*)NativeMemory.Alloc((nuint)this.dataLength, (nuint)sizeof(T));
        this.bufferAsList = new NativeUncheckedArrayList<T>(this.pBuffer, dataLength);
    }

    [IterationSetup]
    public void IterationSetup()
    {
        Buffer.MemoryCopy(this.pData, this.pBuffer, sizeof(float) * dataLength, sizeof(float) * dataLength);
    }

    [Benchmark(Baseline = true)]
    public void SortStandard()
    {
        ListUtil.Sort<T, NativeUncheckedArrayList<T>, TStandardComparer>(this.bufferAsList, default);
    }

    [Benchmark]
    public void SortFullyInlined()
    {
        ListUtil.Sort<T, NativeUncheckedArrayList<T>, TFullyInlinedComparer>(this.bufferAsList, default);
    }

    [Benchmark]
    public void SortSplitInlined()
    {
        ListUtil.Sort<T, NativeUncheckedArrayList<T>, TSplitInlinedComparer>(this.bufferAsList, default);
    }
}