using System;
using NBench;
using TrafficSimulation.Simulations;
using TrafficSimulation.Simulations.CarFollowing;
using TrafficSimulation.Simulations.CellBased;
using TrafficSimulation.Utils;

namespace Benchmarks
{
    /// <summary>
    /// Contains helper methods used in benchmarking
    /// </summary>
    public static class BenchmarkUtils
    {
        public const int BenchmarkIterations = 10;

        public const int WarmupStepCount = 10;
        public const int RunStepCount = 600;
        public const int RandomSeed = 123456;

        /// <summary>
        /// OpenCL Device index used for testing
        /// </summary>
        public const int DefaultOpenCLDeviceIndex = 1;


        private static OpenCLDispatcher dispatcher;
        private static OpenCLDevice device;

        private static Type lastSimType;
        private static int lastDistance, lastJunctionsX, lastJunctionsY, lastCarCount, lastMaxCarCount;
        private static float lastGeneratorProbability;

        static BenchmarkUtils()
        {
            dispatcher = new OpenCLDispatcher();

            device = dispatcher.Devices[DefaultOpenCLDeviceIndex];

            Console.WriteLine("OpenCL Device: " + device.ShortName);
        }

        public static CellBasedSim CreateCellBasedSim(IBenchmarkTrace trace, int distance,
            int junctionsX, int junctionsY, int carCount, int maxCarCount, float generatorProbability)
        {

            PrintBenchmarkInfoIfNeeded<CellBasedSim>(trace, distance,
                junctionsX, junctionsY, carCount, maxCarCount, generatorProbability);

            CellBasedSim sim = new CellBasedSim(RandomSeed);
            sim.GenerateNew(distance, junctionsX, junctionsY, carCount, maxCarCount, generatorProbability, RandomSeed);

            for (int i = 0; i < WarmupStepCount; i++) {
                sim.DoStepReference();
            }

            return sim;
        }

        public static CarFollowingSim CreateCarFollowingSim(IBenchmarkTrace trace, int distance,
            int junctionsX, int junctionsY, int carCount, int maxCarCount, float generatorProbability)
        {
            PrintBenchmarkInfoIfNeeded<CarFollowingSim>(trace, distance,
                junctionsX, junctionsY, carCount, maxCarCount, generatorProbability);

            CarFollowingSim sim = new CarFollowingSim(RandomSeed);
            sim.GenerateNew(distance, junctionsX, junctionsY, carCount, maxCarCount, generatorProbability, RandomSeed);

            for (int i = 0; i < WarmupStepCount; i++) {
                sim.DoStepReference();
            }

            return sim;
        }

        public static void RunReference(SimulationBase sim)
        {
            for (int i = 0; i < RunStepCount; i++) {
                sim.DoStepReference();
            }
        }

        public static void RunOpenCL(SimulationBase sim)
        {
            sim.DoBatchOpenCL(dispatcher, device, RunStepCount);
        }

        public static void Cleanup(SimulationBase sim)
        {
            sim.Dispose();
        }

        private static void PrintBenchmarkInfoIfNeeded<T>(IBenchmarkTrace trace, int distance,
            int junctionsX, int junctionsY, int carCount, int maxCarCount, float generatorProbability) where T : SimulationBase
        {
            if (lastSimType == typeof(T) && lastDistance == distance &&
                lastJunctionsX == junctionsX && lastJunctionsY == junctionsY &&
                lastCarCount == carCount && lastMaxCarCount == maxCarCount &&
                lastGeneratorProbability == generatorProbability) {
                return;
            }

            lastSimType = typeof(T);
            lastDistance = distance;
            lastJunctionsX = junctionsX;
            lastJunctionsY = junctionsY;
            lastCarCount = carCount;
            lastMaxCarCount = maxCarCount;
            lastGeneratorProbability = generatorProbability;

            trace.Info("");

            if (typeof(T) == typeof(CellBasedSim)) {
                trace.Info("Generating cell-based simulation...");
            } else if(typeof(T) == typeof(CarFollowingSim)) {
                trace.Info("Generating car following simulation...");
            } else {
                trace.Info("Generating unknown simulation...");
            }

            trace.Info("- Distance: " + distance);
            trace.Info("- Junctions: " + junctionsX + " x " + junctionsY);
            trace.Info("- Car Count: " + carCount + " / " + maxCarCount);
            trace.Info("- Probability: " + generatorProbability);
            trace.Info("");
            trace.Info("- Warmup Step Count: " + WarmupStepCount);
            trace.Info("- Run Step Count: " + RunStepCount);
            trace.Info("- Benchmark Iterations: " + BenchmarkIterations);
            trace.Info("- Random Seed: " + RandomSeed);
            trace.Info("");
        }
    }
}