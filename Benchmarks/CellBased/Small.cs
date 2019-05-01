using NBench;
using TrafficSimulation.Simulations;

namespace Benchmarks.CellBased
{
    public class Small
    {
        private SimulationBase sim;

        [PerfSetup]
        public void Setup(BenchmarkContext context)
        {
            sim = BenchmarkUtils.CreateCellBasedSim(context.Trace, 24, 10, 10, 500, 3000, 0.003f);
        }

        [PerfBenchmark(Description = "Cell-based simulation - Reference - Small",
            NumberOfIterations = BenchmarkUtils.BenchmarkIterations, RunMode = RunMode.Iterations,
            TestMode = TestMode.Measurement, SkipWarmups = true)]
        [TimingMeasurement]
        [MemoryMeasurement(MemoryMetric.TotalBytesAllocated)]
        public void Reference()
        {
            BenchmarkUtils.RunReference(sim);
        }

        [PerfBenchmark(Description = "Cell-based simulation - OpenCL - Small",
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