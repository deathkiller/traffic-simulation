using System;
using System.IO;
using System.Reflection;
using NBench.Sdk;

namespace Benchmarks
{
    internal static class App
    {
        private static int Main(string[] args)
        {
            string[] include;

            if (args.Length > 0) {
                if (args[0] == "-help" || args[0] == "/help") {
                    ShowHelp();
                    return 0;
                }

                include = GetIncludePatternFromArgs(args[0]);
            } else {
                include = null;
            }

            bool allTestsPassed = RunTests(include);

            Console.WriteLine("Done!");
            Console.ReadLine();

            return (allTestsPassed ? 0 : -1);
        }

        /// <summary>
        /// Process args and create include pattern to run selected tests
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <returns>Include pattern</returns>
        private static string[] GetIncludePatternFromArgs(string args)
        {
            if (args.Length < 1) {
                return null;
            }

            string simType = "*", runType = "*", sizeType = "*";
            switch (args[0]) {
                case 'a': simType = "CellBased"; break;
                case 'b': simType = "CarFollowing"; break;
            }

            if (args.Length >= 2) {
                switch (args[1]) {
                    case 'r': runType = "Reference"; break;
                    case 'o': runType = "OpenCL"; break;
                }

                if (args.Length >= 3) {
                    switch (args[2]) {
                        case 's': sizeType = "Small"; break;
                        case 'm': sizeType = "Medium"; break;
                        case 'l': sizeType = "Large"; break;
                        case 'v': sizeType = "VeryLarge"; break;
                    }
                }
            }

            return new string[] { "*." + simType + "." + sizeType + "+" + runType };
        }

        /// <summary>
        /// Run all or selected tests
        /// </summary>
        /// <param name="include">Include pattern</param>
        /// <returns>All tests passed</returns>
        private static bool RunTests(string[] include)
        {
            if (include == null || include.Length < 1) {
                Console.WriteLine("Running all benchmarks");
            } else {
                Console.WriteLine("Running benchmarks with pattern: " + include[0]);
            }

            Console.WriteLine();

            string path = Assembly.GetExecutingAssembly().Location;
            string resultPath = Path.Combine(Path.GetDirectoryName(path), "Results");

            Directory.CreateDirectory(resultPath);

            TestPackage package = new TestPackage(Assembly.GetExecutingAssembly().Location, include);
            package.Tracing = true;
            package.OutputDirectory = resultPath;

            package.Validate();
            TestRunnerResult result = TestRunner.Run(package);

            return result.AllTestsPassed;
        }

        /// <summary>
        /// Print help to standard output
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Benchmarking tool for Traffic Simulation");
            Console.WriteLine("(c) 2018-2019 Daniel Rajf");
            Console.WriteLine();
            Console.WriteLine("Run without parameters to execute all benchmarks or use following parameters to execute only specified benchmarks.");
            Console.WriteLine(" - X      Execute benchmarks with specified sim. type");
            Console.WriteLine(" - XY     Execute benchmarks with specified sim. type and implementation");
            Console.WriteLine(" - XYZ    Execute benchmarks with specified sim. type, implementation and size");
            Console.WriteLine();
            Console.WriteLine("Letter X can be:");
            Console.WriteLine(" - a      Cell-based simulation");
            Console.WriteLine(" - b      Car-following simulation");
            Console.WriteLine();
            Console.WriteLine("Letter Y can be:");
            Console.WriteLine(" - r      Reference implementation");
            Console.WriteLine(" - o      OpenCL implementation");
            Console.WriteLine();
            Console.WriteLine("Letter Z can be:");
            Console.WriteLine(" - s      Small traffic network");
            Console.WriteLine(" - m      Medium traffic network");
            Console.WriteLine(" - l      Large traffic network");
            Console.WriteLine(" - v      Very large traffic network");
            Console.WriteLine();
            Console.WriteLine("For example, for car-following simulation and OpenCL implementation:");
            Console.WriteLine("  Benckmarks.exe bo");
        }
    }
}