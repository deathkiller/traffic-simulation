using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Simulations.CarFollowing
{
    partial class CarFollowingSim
    {
        /// <inheritdoc />
        public override unsafe void DoStepOpenCL(OpenCLDispatcher dispatcher, OpenCLDevice device)
        {
            var timerTotal = Stopwatch.StartNew();

            OpenCLKernelSet kernelSet = dispatcher.Compile(device, "CarFollowingSim.cl");

            currentStep++;

            // Increase random seed
            randomSeed++;

            int cellsLength = Current.Cells.Length;
            int junctionsLength = Current.Junctions.Length;
            int generatorsLength = Current.Generators.Length;
            int carsLength = Current.Cars.Length;

            int isChanged = 1;

            // Reset waiting count on every junction
            Parallel.ForEach(Partitioner.Create(0, Current.Junctions.Length), range => {
                for (int i = range.Item1; i < range.Item2; i++) {
                    Current.Junctions[i].WaitingCount = 0;
                }
            });

            fixed (Cell* cellsPtr = Current.Cells)
            fixed (int* cellsToCarPtr = Current.CellsToCar)
            fixed (Junction* junctionsPtr = Current.Junctions)
            fixed (Generator* generatorsPtr = Current.Generators)
            fixed (Car* carsPtr = Current.Cars)
            fixed (float* randomPtr = random) {

                var timer = Stopwatch.StartNew();

                // Process all cells
                kernelSet["DoStepCarPre"]
                    .BindBuffer(cellsPtr, sizeof(Cell) * cellsLength, false)
                    .BindBuffer(cellsToCarPtr, sizeof(int) * cellsLength * Current.CarsPerCell, false)
                    .BindValue(cellsLength)

                    .BindBuffer(junctionsPtr, sizeof(Junction) * junctionsLength, false)
                    .BindValue(junctionsLength)

                    .BindBuffer(carsPtr, sizeof(Car) * carsLength, false)
                    .BindValue(carsLength)
                    .BindValue(Current.CarsPerCell)

                    .BindBuffer(randomPtr, sizeof(float) * randomLength, true)
                    .BindValue(randomLength)
                    .BindValue(randomSeed)

                    .BindValue(dt)

                    .Run(cellsLength)
                    .Finish();

                while (isChanged == 1) {
                    isChanged = 0;

                    kernelSet["DoStepCarPost"]
                        .BindBuffer(cellsPtr, sizeof(Cell) * cellsLength, false)
                        .BindBuffer(cellsToCarPtr, sizeof(int) * cellsLength * Current.CarsPerCell, false)
                        .BindValue(cellsLength)

                        .BindBuffer(junctionsPtr, sizeof(Junction) * junctionsLength, false)
                        .BindValue(junctionsLength)

                        .BindBuffer(carsPtr, sizeof(Car) * carsLength, false)
                        .BindValue(carsLength)
                        .BindValue(Current.CarsPerCell)

                        .BindBuffer(randomPtr, sizeof(float) * randomLength, true)
                        .BindValue(randomLength)
                        .BindValue(randomSeed)

                        .BindBuffer(&isChanged, sizeof(int), false)

                        .Run(cellsLength)
                        .Finish();
                }

                LastTimeCars = timer.Elapsed;
                timer.Restart();

                // Process all generators
                if ((flags & SimulationFlags.NoSpawn) == 0) {
                    kernelSet["SpawnCars"]
                        .BindBuffer(cellsPtr, sizeof(Cell) * cellsLength, false)
                        .BindBuffer(cellsToCarPtr, sizeof(int) * cellsLength * Current.CarsPerCell, false)
                        .BindValue(cellsLength)

                        .BindBuffer(generatorsPtr, sizeof(Generator) * generatorsLength, false)
                        .BindValue(generatorsLength)

                        .BindBuffer(carsPtr, sizeof(Car) * carsLength, false)
                        .BindValue(carsLength)
                        .BindValue(Current.CarsPerCell)

                        .BindBuffer(randomPtr, sizeof(float) * randomLength, true)
                        .BindValue(randomLength)
                        .BindValue(randomSeed)

                        .BindValue(dt)

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
            OpenCLKernelSet kernelSet = dispatcher.Compile(device, "CarFollowingSim.cl");

            int cellsLength = Current.Cells.Length;
            int junctionsLength = Current.Junctions.Length;
            int generatorsLength = Current.Generators.Length;
            int carsLength = Current.Cars.Length;

            int isChanged = 0;

            // Reset waiting count on every junction
            /*Parallel.ForEach(Partitioner.Create(0, Current.Junctions.Length), range => {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Current.Junctions[i].WaitingCount = 0;
                }
            });*/

            fixed (Cell* cellsPtr = Current.Cells)
            fixed (int* cellsToCarPtr = Current.CellsToCar)
            fixed (Junction* junctionsPtr = Current.Junctions)
            fixed (Generator* generatorsPtr = Current.Generators)
            fixed (Car* carsPtr = Current.Cars)
            fixed (float* randomPtr = random) {

                var kernelDoStepCarPre = kernelSet["DoStepCarPre"];
                var kernelDoStepCarPost = kernelSet["DoStepCarPost"];
                var kernelSpawnCars = kernelSet["SpawnCars"];

                // Create all buffers
                using (var cellsBuffer = dispatcher.CreateBuffer(device, cellsPtr, sizeof(Cell) * cellsLength, false))
                using (var cellsToCarBuffer = dispatcher.CreateBuffer(device, cellsToCarPtr, sizeof(int) * cellsLength * Current.CarsPerCell, false))
                using (var junctionsBuffer = dispatcher.CreateBuffer(device, junctionsPtr, sizeof(Junction) * junctionsLength, false))
                using (var generatorsBuffer = dispatcher.CreateBuffer(device, generatorsPtr, sizeof(Generator) * generatorsLength, false))
                using (var carsBuffer = dispatcher.CreateBuffer(device, carsPtr, sizeof(Car) * carsLength, false))
                using (var randomBuffer = dispatcher.CreateBuffer(device, randomPtr, sizeof(float) * randomLength, true))
                using (var isChangedBuffer = dispatcher.CreateBuffer(device, &isChanged, sizeof(int), false)) {

                    // Prepare kernels
                    kernelDoStepCarPre
                        .BindBuffer(cellsBuffer)
                        .BindBuffer(cellsToCarBuffer)
                        .BindValue(cellsLength)

                        .BindBuffer(junctionsBuffer)
                        .BindValue(junctionsLength)

                        .BindBuffer(carsBuffer)
                        .BindValue(carsLength)
                        .BindValue(Current.CarsPerCell)

                        .BindBuffer(randomBuffer)
                        .BindValue(randomLength)
                        .BindValue(randomSeed)

                        .BindValue(dt);

                    kernelDoStepCarPost
                        .BindBuffer(cellsBuffer)
                        .BindBuffer(cellsToCarBuffer)
                        .BindValue(cellsLength)

                        .BindBuffer(junctionsBuffer)
                        .BindValue(junctionsLength)

                        .BindBuffer(carsBuffer)
                        .BindValue(carsLength)
                        .BindValue(Current.CarsPerCell)

                        .BindBuffer(randomBuffer)
                        .BindValue(randomLength)
                        .BindValue(randomSeed)

                        .BindBuffer(isChangedBuffer);

                    kernelSpawnCars
                        .BindBuffer(cellsBuffer)
                        .BindBuffer(cellsToCarBuffer)
                        .BindValue(cellsLength)

                        .BindBuffer(generatorsBuffer)
                        .BindValue(generatorsLength)

                        .BindBuffer(carsBuffer)
                        .BindValue(carsLength)
                        .BindValue(Current.CarsPerCell)

                        .BindBuffer(randomBuffer)
                        .BindValue(randomLength)
                        .BindValue(randomSeed)

                        .BindValue(dt);

                    // Call kernels, compute simulation
                    for (int i = 0; i < steps; i++) {
                        currentStep++;

                        // Increase random seed
                        randomSeed++;

                        // Process all cells
                        kernelDoStepCarPre
                            .BindValueByIndex(10, randomSeed)
                            .Run(cellsLength);

                        bool isFirst = true;
                        while (true) {
                            if (isFirst) {
                                isFirst = false;
                            } else {
                                using (isChangedBuffer.MapAsWritable()) {
                                    if (isChanged == 0) {
                                        break;
                                    }

                                    isChanged = 0;
                                }
                            }

                            kernelDoStepCarPost
                                .BindValueByIndex(10, randomSeed)
                                .Run(cellsLength);
                        }

                        // Process all generators
                        if ((flags & SimulationFlags.NoSpawn) == 0) {
                            kernelSpawnCars
                                .BindValueByIndex(10, randomSeed)
                                .Run(generatorsLength);
                        }
                    }

                    // Cleanup
                    kernelDoStepCarPre.Finish();
                    kernelDoStepCarPost.Finish();
                    kernelSpawnCars.Finish();
                }
            }
        }
    }
}