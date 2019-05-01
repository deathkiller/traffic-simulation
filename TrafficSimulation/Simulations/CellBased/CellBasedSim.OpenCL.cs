using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Simulations.CellBased
{
    partial class CellBasedSim
    {
        /// <inheritdoc />
        public override unsafe void DoStepOpenCL(OpenCLDispatcher dispatcher, OpenCLDevice device)
        {
            var timerTotal = Stopwatch.StartNew();

            OpenCLKernelSet kernelSet = dispatcher.Compile(device, "CellBasedSim.cl");

            currentStep++;

            // Increase random seed
            randomSeed++;

            int cellsLength = Current.Cells.Length;
            int junctionsLength = Current.Junctions.Length;
            int generatorsLength = Current.Generators.Length;
            int carsLength = Current.Cars.Length;

            // Reset waiting count on every junction
            Parallel.ForEach(Partitioner.Create(0, Current.Junctions.Length), range => {
                for (int i = range.Item1; i < range.Item2; i++) {
                    Current.Junctions[i].WaitingCount = 0;
                }
            });

            fixed (Cell* cellsPtr = Current.Cells)
            fixed (CellToCar* cellsToCarPtr = Current.CellsToCar)
            fixed (Junction* junctionsPtr = Current.Junctions)
            fixed (Generator* generatorsPtr = Current.Generators)
            fixed (Car* carsPtr = Current.Cars)
            fixed (float* randomPtr = random) {

                var timer = Stopwatch.StartNew();

                // Process all cars
                kernelSet["DoStepCar"]
                    .BindBuffer(cellsPtr, sizeof(Cell) * cellsLength, true)
                    .BindBuffer(cellsToCarPtr, sizeof(CellToCar) * cellsLength, false)
                    .BindValue(cellsLength)

                    .BindBuffer(junctionsPtr, sizeof(Junction) * junctionsLength, false)
                    .BindValue(junctionsLength)

                    .BindBuffer(carsPtr, sizeof(Car) * carsLength, false)
                    .BindValue(carsLength)

                    .BindBuffer(randomPtr, sizeof(float) * randomLength, true)
                    .BindValue(randomLength)
                    .BindValue(randomSeed)

                    .Run(carsLength)
                    .Finish();

                LastTimeCars = timer.Elapsed;
                timer.Restart();

                // Process all generators
                if ((flags & SimulationFlags.NoSpawn) == 0) {
                    kernelSet["SpawnCars"]
                        .BindBuffer(cellsPtr, sizeof(Cell) * cellsLength, true)
                        .BindBuffer(cellsToCarPtr, sizeof(CellToCar) * cellsLength, false)
                        .BindValue(cellsLength)

                        .BindBuffer(generatorsPtr, sizeof(Generator) * generatorsLength, false)
                        .BindValue(generatorsLength)

                        .BindBuffer(carsPtr, sizeof(Car) * carsLength, false)
                        .BindValue(carsLength)

                        .BindBuffer(randomPtr, sizeof(float) * randomLength, true)
                        .BindValue(randomLength)
                        .BindValue(randomSeed)

                        .Run(generatorsLength)
                        .Finish();
                }


                LastTimeGenerators = timer.Elapsed;
            }

            LastTimeTotal = timerTotal.Elapsed;
        }

        /// <inheritdoc />
        public override unsafe void DoBatchOpenCL(OpenCLDispatcher dispatcher, OpenCLDevice device, int steps)
        {
            OpenCLKernelSet kernelSet = dispatcher.Compile(device, "CellBasedSim.cl");

            int cellsLength = Current.Cells.Length;
            int junctionsLength = Current.Junctions.Length;
            int generatorsLength = Current.Generators.Length;
            int carsLength = Current.Cars.Length;

            fixed (Cell* cellsPtr = Current.Cells)
            fixed (CellToCar* cellsToCarPtr = Current.CellsToCar)
            fixed (Junction* junctionsPtr = Current.Junctions)
            fixed (Generator* generatorsPtr = Current.Generators)
            fixed (Car* carsPtr = Current.Cars)
            fixed (float* randomPtr = random) {

                var kernelDoStepCar = kernelSet["DoStepCar"];
                var kernelSpawnCars = kernelSet["SpawnCars"];

                // Create all buffers
                using (var cellsBuffer = dispatcher.CreateBuffer(device, cellsPtr, sizeof(Cell) * cellsLength, true))
                using (var cellsToCarBuffer = dispatcher.CreateBuffer(device, cellsToCarPtr, sizeof(CellToCar) * cellsLength, false))
                using (var junctionsBuffer = dispatcher.CreateBuffer(device, junctionsPtr, sizeof(Junction) * junctionsLength, false))
                using (var generatorsBuffer = dispatcher.CreateBuffer(device, generatorsPtr, sizeof(Generator) * generatorsLength, false))
                using (var carsBuffer = dispatcher.CreateBuffer(device, carsPtr, sizeof(Car) * carsLength, false))
                using (var randomBuffer = dispatcher.CreateBuffer(device, randomPtr, sizeof(float) * randomLength, true)) {

                    // Prepare kernels
                    kernelDoStepCar
                        .BindBuffer(cellsBuffer)
                        .BindBuffer(cellsToCarBuffer)
                        .BindValue(cellsLength)

                        .BindBuffer(junctionsBuffer)
                        .BindValue(junctionsLength)

                        .BindBuffer(carsBuffer)
                        .BindValue(carsLength)

                        .BindBuffer(randomBuffer)
                        .BindValue(randomLength)
                        .BindValue(randomSeed);

                    kernelSpawnCars
                        .BindBuffer(cellsBuffer)
                        .BindBuffer(cellsToCarBuffer)
                        .BindValue(cellsLength)

                        .BindBuffer(generatorsBuffer)
                        .BindValue(generatorsLength)

                        .BindBuffer(carsBuffer)
                        .BindValue(carsLength)

                        .BindBuffer(randomBuffer)
                        .BindValue(randomLength)
                        .BindValue(randomSeed);

                    // Call kernels, compute simulation
                    for (int i = 0; i < steps; i++) {
                        currentStep++;

                        // Increase random seed
                        randomSeed++;

                        // Process all cars
                        kernelDoStepCar
                            .BindValueByIndex(9, randomSeed)
                            .Run(carsLength);

                        // Process all generators
                        if ((flags & SimulationFlags.NoSpawn) == 0) {
                            kernelSpawnCars
                                .BindValueByIndex(9, randomSeed)
                                .Run(generatorsLength);
                        }
                    }

                    // Cleanup
                    kernelDoStepCar.Finish();
                    kernelSpawnCars.Finish();
                }
            }
        }
    }
}