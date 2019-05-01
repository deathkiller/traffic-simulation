using NBench;
using TrafficSimulation.Simulations;

namespace Benchmarks.CellBased
{
    public class Large
    {
        private SimulationBase sim;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            sim = BenchmarkUtils.CreateCellBasedSim(context.Trace, 24, 100, 100, 50000, 300000, 0.01f);
        }

        [PerfBenchmark(Description = "Cell-based simulation - Reference - Large",
            NumberOfIterations = BenchmarkUtils.BenchmarkIterations, RunMode = RunMode.Iterations,
            TestMode = TestMode.Measurement, SkipWarmups = true)]
        [TimingMeasurement]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Reference()
        {
            BenchmarkUtils.RunReference(sim);
        }

        [PerfBenchmark(Description = "Cell-based simulation - OpenCL - Large",
            NumberOfIterations = BenchmarkUtils.BenchmarkIterations, RunMode = RunMode.Iterations,
            TestMode = TestMode.Measurement, SkipWarmups = true)]
        [TimingMeasurement]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void OpenCL()
        {
            BenchmarkUtils.RunOpenCL(sim);
        }

        [PerfCleanup]
        public void Cleanup()
        {
            BenchmarkUtils.Cleanup(sim);
        }
    }
}