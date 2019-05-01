using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TrafficSimulation.Simulations.CellBased
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Cell : IEquatable<Cell> // 48 bytes
    {
        public const int None = -1;
        public const int Terminator = -2;


        public int T1, T2, T3, T4, T5;
        public float P1, P2, P3, P4, P5;
        public int JunctionIndex;
        public int NearestJunctionIndex;

        public override bool Equals(object obj)
        {
            return obj is Cell && Equals((Cell)obj);
        }

        public bool Equals(Cell other)
        {
            return T1 == other.T1 &&
                   T2 == other.T2 &&
                   T3 == other.T3 &&
                   T4 == other.T4 &&
                   T5 == other.T5 &&
                   P1 == other.P1 &&
                   P2 == other.P2 &&
                   P3 == other.P3 &&
                   P4 == other.P4 &&
                   P5 == other.P5 &&
                   JunctionIndex == other.JunctionIndex &&
                   NearestJunctionIndex == other.NearestJunctionIndex;
        }

        public override int GetHashCode()
        {
            var hashCode = 2018318558;
            hashCode = hashCode * -1521134295 + T1.GetHashCode();
            hashCode = hashCode * -1521134295 + T2.GetHashCode();
            hashCode = hashCode * -1521134295 + T3.GetHashCode();
            hashCode = hashCode * -1521134295 + T4.GetHashCode();
            hashCode = hashCode * -1521134295 + T5.GetHashCode();
            hashCode = hashCode * -1521134295 + P1.GetHashCode();
            hashCode = hashCode * -1521134295 + P2.GetHashCode();
            hashCode = hashCode * -1521134295 + P3.GetHashCode();
            hashCode = hashCode * -1521134295 + P4.GetHashCode();
            hashCode = hashCode * -1521134295 + P5.GetHashCode();
            hashCode = hashCode * -1521134295 + JunctionIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + NearestJunctionIndex.GetHashCode();
            return hashCode;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CellToCar : IEquatable<CellToCar> // 4 bytes
    {
        public int CarIndex;

        public override bool Equals(object obj)
        {
            return obj is CellToCar && Equals((CellToCar)obj);
        }

        public bool Equals(CellToCar other)
        {
            return CarIndex == other.CarIndex;
        }

        public override int GetHashCode()
        {
            return 1176724079 + CarIndex.GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Junction : IEquatable<Junction> // 8 bytes
    {
        public int CellIndex;
        public int WaitingCount;

        public override bool Equals(object obj)
        {
            return obj is Junction && Equals((Junction)obj);
        }

        public bool Equals(Junction other)
        {
            return CellIndex == other.CellIndex &&
                   WaitingCount == other.WaitingCount;
        }

        public override int GetHashCode()
        {
            var hashCode = 565124858;
            hashCode = hashCode * -1521134295 + CellIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + WaitingCount.GetHashCode();
            return hashCode;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Generator : IEquatable<Generator> // 12 bytes
    {
        public int CellIndex;
        public float ProbabilityLambda;
        public float TimeLeft;

        public override bool Equals(object obj)
        {
            return obj is Generator && Equals((Generator)obj);
        }

        public bool Equals(Generator other)
        {
            return CellIndex == other.CellIndex &&
                   ProbabilityLambda == other.ProbabilityLambda &&
                   TimeLeft == other.TimeLeft;
        }

        public override int GetHashCode()
        {
            var hashCode = -1578945289;
            hashCode = hashCode * -1521134295 + CellIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + ProbabilityLambda.GetHashCode();
            hashCode = hashCode * -1521134295 + TimeLeft.GetHashCode();
            return hashCode;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Car : IEquatable<Car> // 12 bytes
    {
        public int Position;
        public int Speed;
        public int Size;

        public override bool Equals(object obj)
        {
            return obj is Car && Equals((Car)obj);
        }

        public bool Equals(Car other)
        {
            return Position == other.Position &&
                   Speed == other.Speed &&
                   Size == other.Size;
        }

        public override int GetHashCode()
        {
            var hashCode = -1307787418;
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + Speed.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            return hashCode;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CellUi : IEquatable<CellUi> // 8 bytes
    {
        public int X, Y;

        public override bool Equals(object obj)
        {
            return obj is CellUi && Equals((CellUi)obj);
        }

        public bool Equals(CellUi other)
        {
            return X == other.X &&
                   Y == other.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct CarUi : IEquatable<CarUi> // 4 bytes
    {
        public int Color;

        public override bool Equals(object obj)
        {
            return obj is CarUi && Equals((CarUi)obj);
        }

        public bool Equals(CarUi other)
        {
            return Color == other.Color;
        }

        public override int GetHashCode()
        {
            return -1200350280 + Color.GetHashCode();
        }
    }

    public struct SimulationData : IEquatable<SimulationData>
    {
        public Cell[] Cells;
        public CellToCar[] CellsToCar;

        public Junction[] Junctions;
        public Generator[] Generators;

        public Car[] Cars;

        public CellUi[] CellsUi;

        public CarUi[] CarsUi;

        public override bool Equals(object obj)
        {
            return obj is SimulationData && Equals((SimulationData)obj);
        }

        public bool Equals(SimulationData other)
        {
            return EqualityComparer<Cell[]>.Default.Equals(Cells, other.Cells) &&
                   EqualityComparer<CellToCar[]>.Default.Equals(CellsToCar, other.CellsToCar) &&
                   EqualityComparer<Junction[]>.Default.Equals(Junctions, other.Junctions) &&
                   EqualityComparer<Generator[]>.Default.Equals(Generators, other.Generators) &&
                   EqualityComparer<Car[]>.Default.Equals(Cars, other.Cars) &&
                   EqualityComparer<CellUi[]>.Default.Equals(CellsUi, other.CellsUi) &&
                   EqualityComparer<CarUi[]>.Default.Equals(CarsUi, other.CarsUi);
        }

        public override int GetHashCode()
        {
            var hashCode = -534996872;
            hashCode = hashCode * -1521134295 + EqualityComparer<Cell[]>.Default.GetHashCode(Cells);
            hashCode = hashCode * -1521134295 + EqualityComparer<CellToCar[]>.Default.GetHashCode(CellsToCar);
            hashCode = hashCode * -1521134295 + EqualityComparer<Junction[]>.Default.GetHashCode(Junctions);
            hashCode = hashCode * -1521134295 + EqualityComparer<Generator[]>.Default.GetHashCode(Generators);
            hashCode = hashCode * -1521134295 + EqualityComparer<Car[]>.Default.GetHashCode(Cars);
            hashCode = hashCode * -1521134295 + EqualityComparer<CellUi[]>.Default.GetHashCode(CellsUi);
            hashCode = hashCode * -1521134295 + EqualityComparer<CarUi[]>.Default.GetHashCode(CarsUi);
            return hashCode;
        }
    }

    public partial class CellBasedSim : SimulationBase
    {
        private const int MaxCarSize = 10;
        private const int SpeedLimit = 6; // 54 km/h
        private const float SlowdownProbability = 0.2f;

        public SimulationData Current;

        public TimeSpan LastTimeTotal;
        public TimeSpan LastTimeCars;
        public TimeSpan LastTimeGenerators;

        public int ActiveCarsCount;
        public int WaitingCarsCount;

        private const int randomLength = 1024 * 1024;
        private float[] random = new float[randomLength];
        private int randomSeed = 1;

        private SimulationFlags flags;
        private long currentStep = 0;

        /// <inheritdoc />
        public override SimulationType CurrentType => SimulationType.CellBased;

        /// <inheritdoc />
        public override bool IsReady => (Current.Cells != null);

        /// <inheritdoc />
        public override long CurrentStep
        {
            get
            {
                return currentStep;
            }
            protected set
            {
                currentStep = value;
            }
        }

        /// <inheritdoc />
        public override SimulationFlags Flags { get => flags; set => flags = value; }

        public CellBasedSim(int? randomSeed = null)
        {
            Random r;
            if (randomSeed == null) {
                r = new Random();
            } else {
                r = new Random(randomSeed.Value);
            }

            // Initialize random numbers
            random = new float[randomLength];
            for (int i = 0; i < randomLength; i++) {
                random[i] = (float)r.NextDouble();
            }
        }

        public override string ToString()
        {
            string result = "Junctions: " + Current.Junctions.Length.ToString("N0") +
                "\nCells: " + Current.Cells.Length.ToString("N0") +
                "\n\nCars:\n - Total:" + Current.Cars.Length.ToString("N0");

            if (ActiveCarsCount != 0) {
                result += "\n - Spawned: " + ActiveCarsCount.ToString("N0") +
                    "\n - Waiting: " + WaitingCarsCount.ToString("N0");
            }

            if (LastTimeTotal != TimeSpan.Zero) {
                result += "\n\nElapsed: " + LastTimeTotal.TotalMilliseconds.ToString("N1") + " ms";

                if (LastTimeCars != TimeSpan.Zero) {
                    result += "\n - Cars: " + LastTimeCars.TotalMilliseconds.ToString("N1") + " ms";
                }
                if (LastTimeGenerators != TimeSpan.Zero) {
                    result += "\n - Generators: " + LastTimeGenerators.TotalMilliseconds.ToString("N1") + " ms";
                }
            }

            return result;
        }

        /// <inheritdoc />
        public override void DoStepReference()
        {
            var timerTotal = Stopwatch.StartNew();

            currentStep++;

            // Increase random seed
            randomSeed++;

            // Reset waiting count on every junction
            Parallel.ForEach(Partitioner.Create(0, Current.Junctions.Length), range => {
                for (int i = range.Item1; i < range.Item2; i++) {
                    Current.Junctions[i].WaitingCount = 0;
                }
            });

            var timer = Stopwatch.StartNew();

            // Process all cars
            Parallel.ForEach(Partitioner.Create(0, Current.Cars.Length), range => {
                for (int i = range.Item1; i < range.Item2; i++) {
                    DoStepCar(i);
                }
            });

            LastTimeCars = timer.Elapsed;
            timer.Restart();

            // Process all generators
            if ((flags & SimulationFlags.NoSpawn) == 0) {
                Parallel.ForEach(Partitioner.Create(0, Current.Generators.Length), range => {
                    for (int i = range.Item1; i < range.Item2; i++) {
                        SpawnCars(i);
                    }
                });
            }

            LastTimeGenerators = timer.Elapsed;

            LastTimeTotal = timerTotal.Elapsed;
        }

        /// <summary>
        /// Finds all cells occupied by specific car
        /// </summary>
        /// <param name="i">Car index</param>
        /// <param name="car">Car</param>
        /// <param name="carCells">Target buffer to save cell indices</param>
        private void FindCarCells(int i, ref Car car, int[] carCells)
        {
            int currentCell = 0;

            carCells[currentCell++] = car.Position;

            Cell cell = Current.Cells[car.Position];

            int remaining = car.Size - 1;
            while (remaining > 0) {
                if (cell.T1 != Cell.None && Current.CellsToCar[cell.T1].CarIndex == i) {
                    carCells[currentCell++] = cell.T1;
                    cell = Current.Cells[cell.T1];
                } else if (cell.T2 != Cell.None && Current.CellsToCar[cell.T2].CarIndex == i) {
                    carCells[currentCell++] = cell.T2;
                    cell = Current.Cells[cell.T2];
                } else if (cell.T3 != Cell.None && Current.CellsToCar[cell.T3].CarIndex == i) {
                    carCells[currentCell++] = cell.T3;
                    cell = Current.Cells[cell.T3];
                } else if (cell.T4 != Cell.None && Current.CellsToCar[cell.T4].CarIndex == i) {
                    carCells[currentCell++] = cell.T4;
                    cell = Current.Cells[cell.T4];
                } else if (cell.T5 != Cell.None && Current.CellsToCar[cell.T5].CarIndex == i) {
                    carCells[currentCell++] = cell.T5;
                    cell = Current.Cells[cell.T5];
                }

                remaining--;
            }

            carCells[currentCell] = Cell.None;
        }

        /// <summary>
        /// Finds next cell for car to switch to
        /// </summary>
        /// <param name="c">Current cell</param>
        /// <param name="randomIndex">Randomization index</param>
        /// <returns>Index of next cell</returns>
        private int FindNextCell(ref Cell c, uint randomIndex)
        {
            float rand = random[randomIndex % randomLength];

            float current = c.P1;
            if (rand <= current && c.T1 != Cell.None) return c.T1;
            current += c.P2;
            if (rand <= current && c.T2 != Cell.None) return c.T2;
            current += c.P3;
            if (rand <= current && c.T3 != Cell.None) return c.T3;
            current += c.P4;
            if (rand <= current && c.T4 != Cell.None) return c.T4;

            return c.T5;
        }

        /// <summary>
        /// Fills buffer with indices of cells in front of car
        /// </summary>
        /// <param name="carIdx">Car index</param>
        /// <param name="cells">List of cells</param>
        /// <param name="headCellIdx">Index of first cell with specified car</param>
        /// <param name="range">Number of cells in front to find</param>
        private void FillFrontCells(int carIdx, int[] cells, int headCellIdx, int range)
        {
            for (int i = 0; i < range; i++) {
                ref Cell cell = ref Current.Cells[headCellIdx];

                int idx = FindNextCell(ref cell, (uint)(randomSeed + carIdx * headCellIdx));
                if (idx == Cell.None) {
                    for (; i < SpeedLimit + 1; i++) {
                        cells[i] = Cell.None;
                    }
                    break;
                }

                cells[i] = idx;
                headCellIdx = idx;
            }
        }

        /// <summary>
        /// Executes movement of car
        /// </summary>
        /// <param name="i">Car index/param>
        private void DoStepCar(int i)
        {
            ref Car car = ref Current.Cars[i];
            if (car.Position == Cell.None) {
                return;
            }

            int[] carCells = new int[MaxCarSize];
            int[] frontCells = new int[SpeedLimit + 1];

            // Find all cells occupied by car
            {
                FindCarCells(i, ref car, carCells);

                FillFrontCells(i, frontCells, carCells[car.Size - 1], SpeedLimit + 1);

                bool canAccelerate = true;
                int j = 0;
                for (; j < car.Speed; j++) {
                    if (frontCells[j] == Cell.None) {
                        canAccelerate = false;
                        car.Speed = j;
                        break;
                    }

                    ref CellToCar c = ref Current.CellsToCar[frontCells[j]];
                    if (c.CarIndex != Cell.None) {
                        canAccelerate = false;
                        car.Speed = j;
                        break;
                    }
                }

                if (car.Speed < SpeedLimit && canAccelerate && frontCells[car.Speed] != Cell.None) {
                    ref CellToCar c = ref Current.CellsToCar[frontCells[car.Speed]];
                    if (c.CarIndex == Cell.None) {
                        car.Speed++;
                    }
                }

                // Random slowdown
                if (car.Speed >= 1) {
                    float rand = random[(uint)(randomSeed + i * carCells[0]) % randomLength];
                    if (rand < SlowdownProbability) {
                        car.Speed--;
                    }
                }
            }

            // Move car by specified cells
            for (int j = 0; j < car.Speed; j++) {
                int nextIdx = frontCells[j];

                ref Cell nextCell = ref Current.Cells[nextIdx];
                ref CellToCar nextCellToCar = ref Current.CellsToCar[nextIdx];

                if (nextCell.JunctionIndex == Cell.Terminator) {
                    // Car is at terminator, remove it
                    car.Position = Cell.None;
                    car.Speed = 0;

                    for (int k = 0; k < car.Size; k++) {
                        Current.CellsToCar[carCells[k]].CarIndex = Cell.None;
                    }
                    break;
                }

                if (Interlocked.CompareExchange(ref nextCellToCar.CarIndex, i, Cell.None) != Cell.None) {
                    car.Speed = 0;
                    break;
                }

                Current.CellsToCar[carCells[0]].CarIndex = Cell.None;

                if (car.Size > 1) {
                    car.Position = carCells[1];

                    for (int k = 1; k < car.Size; k++) {
                        carCells[k - 1] = carCells[k];
                    }
                }
                else {
                    car.Position = nextIdx;
                }

                carCells[car.Size - 1] = nextIdx;
            }

            if (car.Speed == 0) {
                ref Cell headCell = ref Current.Cells[carCells[car.Size - 1]];
                if (headCell.NearestJunctionIndex != Cell.None) {
                    ref Junction junction = ref Current.Junctions[headCell.NearestJunctionIndex];
                    Interlocked.Increment(ref junction.WaitingCount);
                }
            }
        }

        /// <summary>
        /// Generates random number with exponential distribution
        /// </summary>
        /// <param name="lambda">Lambda parameter</param>
        /// <param name="randomIndex">Randomization index</param>
        /// <returns>Random number</returns>
        private float RandomExp(float lambda, int randomIndex)
        {
            float rand = random[(uint)randomIndex % randomLength];
            return (float)(-Math.Log(1 - rand) / lambda);
        }

        /// <summary>
        /// Spawns new cars on generators
        /// </summary>
        /// <param name="i">Generator index</param>
        private void SpawnCars(int i)
        {
            ref Generator generator = ref Current.Generators[i];

            if (generator.TimeLeft > 0) {
                // One fixed step
                generator.TimeLeft -= 1;
                return;
            }

            generator.TimeLeft = RandomExp(generator.ProbabilityLambda, randomSeed * (i + 1));

            if (Current.CellsToCar[generator.CellIndex].CarIndex != Cell.None) {
                // There is not enough space for new car
                return;
            }

            int carSize = 1 + (int)(random[(uint)(randomSeed * (i + 1) * 2) % randomLength] * 6);

            int[] frontCells = new int[SpeedLimit + 1];
            FillFrontCells(i, frontCells, generator.CellIndex, carSize - 1);

            for (int j = 0; j < carSize - 1; j++) {
                if (frontCells[j] == Cell.None || Current.CellsToCar[frontCells[j]].CarIndex != Cell.None) {
                    // There is not enough space for new car
                    return;
                }
            }

            int jo = (Current.Cars.Length / 2) + (10 * i);
            for (int j = 0; j < Current.Cars.Length; j++) {
                int carIdx = (jo + j) % Current.Cars.Length;
                ref Car car = ref Current.Cars[carIdx];

                if (Interlocked.CompareExchange(ref car.Position, generator.CellIndex, Cell.None) != Cell.None) {
                    // This car is already spawned
                    continue;
                }

                car.Speed = 0;
                car.Size = carSize;

                Current.CellsToCar[generator.CellIndex].CarIndex = carIdx;
                for (int k = 0; k < carSize - 1; k++) {
                    Current.CellsToCar[frontCells[k]].CarIndex = carIdx;
                }

                break;
            }
        }

        /// <inheritdoc />
        public override bool CheckIntegrity()
        {
            int[] carCells = new int[10];
            int brokenCount = 0;

            ActiveCarsCount = 0;
            WaitingCarsCount = 0;

            for (int i = 0; i < Current.Cars.Length; i++) {
                ref Car car = ref Current.Cars[i];
                if (car.Position == Cell.None) {
                    continue;
                }

                ActiveCarsCount++;

                FindCarCells(i, ref car, carCells);

                for (int j = 0; j < car.Size; j++) {
                    if (carCells[j] == Cell.None) {
                        brokenCount++;
                        break;
                    }
                }
            }

            for (int i = 0; i < Current.Junctions.Length; i++) {
                WaitingCarsCount += Current.Junctions[i].WaitingCount;
            }

            return (brokenCount == 0);
        }

        /// <inheritdoc />
        protected override void InnerLoadFromStream(BinaryReader r)
        {
            Current = new SimulationData();

            // Cells
            {
                int count = r.ReadInt32();
                Current.Cells = new Cell[count];
                Current.CellsToCar = new CellToCar[count];
                Current.CellsUi = new CellUi[count];

                for (int i = 0; i < count; i++) {
                    ref Cell cell = ref Current.Cells[i];
                    cell.T1 = r.ReadInt32();
                    cell.T2 = r.ReadInt32();
                    cell.T3 = r.ReadInt32();
                    cell.T4 = r.ReadInt32();
                    cell.T5 = r.ReadInt32();

                    cell.P1 = r.ReadSingle();
                    cell.P2 = r.ReadSingle();
                    cell.P3 = r.ReadSingle();
                    cell.P4 = r.ReadSingle();
                    cell.P5 = r.ReadSingle();

                    cell.JunctionIndex = r.ReadInt32();
                    cell.NearestJunctionIndex = r.ReadInt32();

                    ref CellToCar cellToCar = ref Current.CellsToCar[i];
                    cellToCar.CarIndex = r.ReadInt32();

                    ref CellUi cellUi = ref Current.CellsUi[i];
                    cellUi.X = r.ReadInt32();
                    cellUi.Y = r.ReadInt32();
                }
            }

            // Junctions
            {
                int count = r.ReadInt32();
                Current.Junctions = new Junction[count];

                for (int i = 0; i < count; i++) {
                    ref Junction junction = ref Current.Junctions[i];

                    junction.CellIndex = r.ReadInt32();
                    junction.WaitingCount = r.ReadInt32();
                }
            }

            // Generators
            {
                int count = r.ReadInt32();
                Current.Generators = new Generator[count];

                for (int i = 0; i < count; i++) {
                    ref Generator generator = ref Current.Generators[i];

                    generator.CellIndex = r.ReadInt32();
                    generator.ProbabilityLambda = r.ReadSingle();
                    generator.TimeLeft = r.ReadSingle();
                }
            }

            // Cars
            {
                int count = r.ReadInt32();
                Current.Cars = new Car[count];
                Current.CarsUi = new CarUi[count];

                for (int i = 0; i < count; i++) {
                    ref Car car = ref Current.Cars[i];
                    car.Position = r.ReadInt32();
                    car.Speed = r.ReadInt32();
                    car.Size = r.ReadInt32();

                    ref CarUi carUi = ref Current.CarsUi[i];
                    carUi.Color = r.ReadInt32();
                }
            }

            LastTimeTotal = TimeSpan.Zero;
            LastTimeCars = TimeSpan.Zero;
            LastTimeGenerators = TimeSpan.Zero;
            ActiveCarsCount = 0;
            WaitingCarsCount = 0;
        }

        /// <inheritdoc />
        protected override void InnerSaveToStream(BinaryWriter w)
        {
            // Cells
            {
                w.Write(Current.Cells.Length);

                for (int i = 0; i < Current.Cells.Length; i++) {
                    ref Cell cell = ref Current.Cells[i];
                    w.Write(cell.T1);
                    w.Write(cell.T2);
                    w.Write(cell.T3);
                    w.Write(cell.T4);
                    w.Write(cell.T5);

                    w.Write(cell.P1);
                    w.Write(cell.P2);
                    w.Write(cell.P3);
                    w.Write(cell.P4);
                    w.Write(cell.P5);

                    w.Write(cell.JunctionIndex);
                    w.Write(cell.NearestJunctionIndex);

                    ref CellToCar cellToCar = ref Current.CellsToCar[i];
                    w.Write(cellToCar.CarIndex);

                    ref CellUi cellUi = ref Current.CellsUi[i];
                    w.Write(cellUi.X);
                    w.Write(cellUi.Y);
                }
            }

            // Junctions
            {
                w.Write(Current.Junctions.Length);

                for (int i = 0; i < Current.Junctions.Length; i++) {
                    ref Junction junction = ref Current.Junctions[i];

                    w.Write(junction.CellIndex);
                    w.Write(junction.WaitingCount);
                }
            }

            // Generators
            {
                w.Write(Current.Generators.Length);

                for (int i = 0; i < Current.Generators.Length; i++) {
                    ref Generator generator = ref Current.Generators[i];

                    w.Write(generator.CellIndex);
                    w.Write(generator.ProbabilityLambda);
                    w.Write(generator.TimeLeft);
                }
            }

            // Cars
            {
                w.Write(Current.Cars.Length);

                for (int i = 0; i < Current.Cars.Length; i++) {
                    ref Car car = ref Current.Cars[i];
                    w.Write(car.Position);
                    w.Write(car.Speed);
                    w.Write(car.Size);

                    ref CarUi carUi = ref Current.CarsUi[i];
                    w.Write(carUi.Color);
                }
            }
        }
    }
}