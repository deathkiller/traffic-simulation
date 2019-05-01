using NBench;
using TrafficSimulation.Simulations;

namespace Benchmarks.CellBased
{
    public class Medium
    {
        private SimulationBase sim;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            sim = BenchmarkUtils.CreateCellBasedSim(context.Trace, 24, 50, 50, 12000, 72000, 0.005f);
        }

        [PerfBenchmark(Description = "Cell-based simulation - Reference - Medium",
            NumberOfIterations = BenchmarkUtils.BenchmarkIterations, RunMode = RunMode.Iterations,
            TestMode = TestMode.Measurement, SkipWarmups = true)]
        [TimingMeasurement]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Reference()
        {
            BenchmarkUtils.RunReference(sim);
        }

        [PerfBenchmark(Description = "Cell-based simulation - OpenCL - Medium",
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