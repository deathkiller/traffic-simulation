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
            string[] include = null;

            if (args.Length > 0) {
                if (args[0] == "-help" || args[0] == "/help") {
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
                    return 0;
                }

                string codename = args[0];
                if (codename.Length >= 1) {
                    string simType = "*", runType = "*", sizeType = "*";
                    switch (codename[0]) {
                        case 'a': simType = "CellBased"; break;
                        case 'b': simType = "CarFollowing"; break;
                    }

                    if (codename.Length >= 2) {
                        switch (codename[1]) {
                            case 'r': runType = "Reference"; break;
                            case 'o': runType = "OpenCL"; break;
                        }

                        if (codename.Length >= 3) {
                            switch (codename[2]) {
                                case 's': sizeType = "Small"; break;
                                case 'm': sizeType = "Medium"; break;
                                case 'l': sizeType = "Large"; break;
                                case 'v': sizeType = "VeryLarge"; break;
                            }
                        }
                    }

                    string pattern = "*." + simType + "." + sizeType + "+" + runType;

                    Console.WriteLine("Running benchmarks with pattern: " + pattern);

                    include = new string[] { pattern };
                }
            } else {
                Console.WriteLine("Running all benchmarks");
            }

            Console.WriteLine();

            string path = Assembly.GetExecutingAssembly().Location;
            string resultPath = Path.Combine(Path.GetDirectoryName(path), "Results");

            Directory.CreateDirectory(resultPath);

            TestPackage package = new TestPackage(Assembly.GetExecutingAssembly().Location, include);
            package.Tracing = true;
            package.OutputDirectory = resultPath;

            package.Validate();
            var result = TestRunner.Run(package);

            Console.WriteLine("Done!");
            Console.ReadLine();

            return (result.AllTestsPassed ? 0 : -1);
        }
    }
}