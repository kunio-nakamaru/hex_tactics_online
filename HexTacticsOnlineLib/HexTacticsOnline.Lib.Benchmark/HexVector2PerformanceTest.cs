using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using HexTacticsOnline.Lib;

namespace HexTacticsOnline.Lib.Benchmark
{
    public class HexVector2PerformanceTest
    {
        private HexVector2 hexVector;
        private List<HexVector2> possibilities;
        private HexVector2 target;

        [GlobalSetup]
        public void Setup()
        {
            hexVector = new HexVector2(0, 0);
            possibilities = new List<HexVector2>
            {
                new HexVector2(1, 1),
                new HexVector2(2, 1),
                new HexVector2(1, 2)
            };
            target = new HexVector2(2, 2);
        }

   

        [Benchmark]
        public void Benchmark_GetRandomAround()
        {
            var exception = new List<HexVector2>();
            var result = hexVector.GetRandomAround(ref exception);
        }

        [Benchmark]
        public void Benchmark_SetUnAllocated()
        {
            hexVector.SetUnAllocated();
        }

        [Benchmark]
        public void Benchmark_IsAllocated()
        {
            var result = hexVector.IsAllocated;
        }

        [Benchmark(Description = "Benchmark_GetPositionForDirection")]
        public void Benchmark_GetPositionForDirection()
        {
            var result = hexVector.GetPositionForDirection(0);
        }

        [Benchmark(Description = "GetClosestPositionForDestination")]
        public void Benchmark_GetClosestPositionForDestination()
        {
            var result = hexVector.GetClosestPositionForDestination(target, possibilities);
        }
        

        [Benchmark(Description = "Distance")]
        public void Benchmark_HexDistance()
        {
            var result = hexVector.HexDistance(target);
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<HexVector2PerformanceTest>();
        }
    }
}