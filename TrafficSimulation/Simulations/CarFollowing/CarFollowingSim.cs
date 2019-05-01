using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TrafficSimulation.Simulations.CarFollowing
{
    /// <summary>
    /// Represents cell
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Cell : IEquatable<Cell> // 56 bytes
    {
        public const int None = -1;
        public const int Terminator = -2;


        public int T1, T2, T3, T4, T5;
        public float P1, P2, P3, P4, P5;
        public int JunctionIndex;
        public int NearestJunctionIndex;

        public float Length;
        public int Lock;

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
                   NearestJunctionIndex == other.NearestJunctionIndex &&
                   Length == other.Length &&
                   Lock == other.Lock;
        }

        public override int GetHashCode()
        {
            var hashCode = -1044093539;
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
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            hashCode = hashCode * -1521134295 + Lock.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Represents junction
    /// </summary>
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

    /// <summary>
    /// Represents generator
    /// </summary>
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

    /// <summary>
    /// Represents car
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Car : IEquatable<Car> // 24 bytes
    {
        public int Position;
        public float PositionInCell;
        public float Speed;
        public int Size;
        public int AlreadyProcessed;

        public override bool Equals(object obj)
        {
            return obj is Car && Equals((Car)obj);
        }

        public bool Equals(Car other)
        {
            return Position == other.Position &&
                   PositionInCell == other.PositionInCell &&
                   Speed == other.Speed &&
                   Size == other.Size &&
                   AlreadyProcessed == other.AlreadyProcessed;
        }

        public override int GetHashCode()
        {
            var hashCode = 1021947672;
            hashCode = hashCode * -1521134295 + Position.GetHashCode();
            hashCode = hashCode * -1521134295 + PositionInCell.GetHashCode();
            hashCode = hashCode * -1521134295 + Speed.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            hashCode = hashCode * -1521134295 + AlreadyProcessed.GetHashCode();
            return hashCode;
        }
    }

    /// <summary>
    /// Represents cell in user interface
    /// </summary>
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

    /// <summary>
    /// Represents car in user interface
    /// </summary>
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

    /// <summary>
    /// Represents current simulation state
    /// </summary>
    public struct SimulationData
    {
        public Cell[] Cells;
        public int[] CellsToCar;

        public Junction[] Junctions;
        public Generator[] Generators;

        public Car[] Cars;

        public CellUi[] CellsUi;

        public CarUi[] CarsUi;

        public int CarsPerCell;
    }

    /// <summary>
    /// Car following simulation
    /// </summary>
    public partial class CarFollowingSim : SimulationBase
    {
        private const int SpeedLimit = 6; // 54 km/h
        private const float SlowdownProbability = 0.2f;

        private const int CellUnlocked = 0;
        private const int CellLocked = 1;

        private const float dt = 1; // 1 sec.

        private const int randomLength = 1024 * 1024;
        private float[] random = new float[randomLength];
        private int randomSeed = 1;

        private SimulationFlags flags;
        private long currentStep;

        public SimulationData Current;

        public TimeSpan LastTimeTotal;
        public TimeSpan LastTimeCars;
        public TimeSpan LastTimeGenerators;

        public int ActiveCarsCount;
        public int WaitingCarsCount;

        /// <inheritdoc />
        public override SimulationType CurrentType => SimulationType.CarFollowing;

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

        public CarFollowingSim(int? randomSeed = null)
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
            Parallel.ForEach(Partitioner.Create(0, Current.Cells.Length), range => {
                for (int i = range.Item1; i < range.Item2; i++) {
                    DoStepCarPre(dt, i);
                }
            });

            int isChanged;
            do {
                isChanged = 0;

                Parallel.ForEach(Partitioner.Create(0, Current.Cells.Length), range => {
                    for (int i = range.Item1; i < range.Item2; i++) {
                        DoStepCarPost(i, ref isChanged);
                    }
                });
            } while (isChanged == 1);

            LastTimeCars = timer.Elapsed;
            timer.Restart();

            // Process all generators
            if ((flags & SimulationFlags.NoSpawn) == 0) {
                Parallel.ForEach(Partitioner.Create(0, Current.Generators.Length), range => {
                    for (int i = range.Item1; i < range.Item2; i++) {
                        SpawnCars(dt, i);
                    }
                });
            }

            LastTimeGenerators = timer.Elapsed;

            LastTimeTotal = timerTotal.Elapsed;
        }

        /// <summary>
        /// Executes movement of car in its traffic lane
        /// </summary>
        /// <param name="dt">Delta time</param>
        /// <param name="i">Traffic lane (cell) index</param>
        private void DoStepCarPre(float dt, int i)
        {
            for (int j = 0; j < Current.CarsPerCell; j++) {
                int carIdx = Current.CellsToCar[i * Current.CarsPerCell + j];
                if (carIdx == Cell.None) {
                    break;
                }

                ref Car car = ref Current.Cars[carIdx];

                car.AlreadyProcessed = 0;

                car.PositionInCell += car.Speed * dt;

                if (car.Speed < 0.1f) {
                    ref Cell headCell = ref Current.Cells[i];
                    if (headCell.NearestJunctionIndex != Cell.None) {
                        ref Junction junction = ref Current.Junctions[headCell.NearestJunctionIndex];
                        Interlocked.Increment(ref junction.WaitingCount);
                    }
                }

                int leaderIdx;
                if (j == 0) {
                    leaderIdx = -1;
                } else {
                    leaderIdx = Current.CellsToCar[i * Current.CarsPerCell + (j - 1)];
                }

                CarFollowing(dt, carIdx, leaderIdx);
            }
        }

        /// <summary>
        /// Executes switching of traffic lanes (cells)
        /// </summary>
        /// <param name="i">Traffic lane (cell) index</param>
        /// <param name="isChanged">Lane (cell) of any car was changed</param>
        private void DoStepCarPost(int i, ref int isChanged)
        {
            for (int j = 0; j < Current.CarsPerCell; j++) {
                int carIdx = Current.CellsToCar[i * Current.CarsPerCell + j];
                if (carIdx == Cell.None) {
                    break;
                }

                ref Car car = ref Current.Cars[carIdx];
                if (Interlocked.Exchange(ref car.AlreadyProcessed, 1) != 0) {
                    continue;
                }

                if (car.PositionInCell >= Current.Cells[i].Length) {
                    // Car is past the current cell
                    int nextIdx = FindNextCell(ref Current.Cells[i], (uint)(randomSeed + i + j));
                    ref Cell nextCell = ref Current.Cells[nextIdx];

                    if (nextCell.JunctionIndex == Cell.Terminator) {
                        // Car is at terminator, remove it
                        if (!RemoveFromCellsToCar(i, j)) {
                            Interlocked.Exchange(ref car.AlreadyProcessed, 0);
                            Interlocked.Exchange(ref isChanged, 1);
                            continue;
                        }

                        j--;

                        car.Position = Cell.None;
                        car.PositionInCell = 0;
                        car.Speed = 0;
                        continue;
                    }

                    // Try lock destination cell
                    if (!TryLockCell(nextIdx)) {
                        Interlocked.Exchange(ref car.AlreadyProcessed, 0);
                        Interlocked.Exchange(ref isChanged, 1);
                        continue;
                    }

                    bool success = false;
                    for (int k = 0; k < Current.CarsPerCell; k++) {
                        int idx = nextIdx * Current.CarsPerCell + k;
                        if (Current.CellsToCar[idx] == Cell.None) {
                            Current.CellsToCar[idx] = carIdx;

                            float additionalDistance = (car.PositionInCell - Current.Cells[i].Length);
                            if (k == 0) {
                                // Lane is empty
                                if (!RemoveFromCellsToCar(i, j)) {
                                    success = true;
                                    Current.CellsToCar[idx] = Cell.None;
                                    break;
                                }

                                j--;

                                car.Position = nextIdx;
                                car.PositionInCell = additionalDistance;

                                success = true;
                            } else {
                                ref Car leader = ref Current.Cars[Current.CellsToCar[idx - 1]];
                                float gap = (leader.PositionInCell - leader.Size);

                                if (additionalDistance <= gap) {
                                    // Gap between vehicles is big enough
                                    if (!RemoveFromCellsToCar(i, j)) {
                                        success = true;
                                        Current.CellsToCar[idx] = Cell.None;
                                        break;
                                    }

                                    j--;

                                    car.Position = nextIdx;
                                    car.PositionInCell = additionalDistance;

                                    success = true;
                                } else if (gap >= 0) {
                                    // Vehicle must decelerate
                                    if (!RemoveFromCellsToCar(i, j)) {
                                        success = true;
                                        Current.CellsToCar[idx] = Cell.None;
                                        break;
                                    }

                                    j--;

                                    car.Position = nextIdx;
                                    car.PositionInCell = gap;
                                    car.Speed = Math.Min(car.Speed, gap);

                                    success = true;
                                } else {
                                    // There is not enough space, transfer is not possible
                                    // Do rollback
                                    Current.CellsToCar[idx] = Cell.None;
                                }
                            }
                            break;
                        }
                    }

                    UnlockCell(nextIdx);

                    if (success) {
                        // Current cell has changed
                        Interlocked.Exchange(ref car.AlreadyProcessed, 0);
                        Interlocked.Exchange(ref isChanged, 1);
                    } else {
                        // Current cell is still the same, next cell is blocked
                        car.Speed = 0;
                    }
                } else {
                    // Car is still in the current cell
                }
            }
        }

        /// <summary>
        /// Removes car from traffic lane (cell)
        /// </summary>
        /// <param name="cellIdx">Traffic lane (cell) index</param>
        /// <param name="position">Car index in lane (cell)</param>
        /// <returns>True if success; false, otherwise</returns>
        private bool RemoveFromCellsToCar(int cellIdx, int position)
        {
            if (!TryLockCell(cellIdx)) {
                return false;
            }

            for (int i = position; i < Current.CarsPerCell - 1; i++) {
                int idx = cellIdx * Current.CarsPerCell + i;
                Current.CellsToCar[idx] = Current.CellsToCar[idx + 1];
                if (Current.CellsToCar[idx] == Cell.None) {
                    UnlockCell(cellIdx);
                    return true;
                }
            }

            Current.CellsToCar[cellIdx * Current.CarsPerCell + Current.CarsPerCell - 1] = Cell.None;
            UnlockCell(cellIdx);
            return true;
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
        /// Tries lock cell before switching
        /// </summary>
        /// <param name="cellIdx">Cell index</param>
        /// <returns>True if success; false, otherwise</returns>
        private bool TryLockCell(int cellIdx)
        {
            return (Interlocked.Exchange(ref Current.Cells[cellIdx].Lock, CellLocked) == CellUnlocked);
        }

        /// <summary>
        /// Unlock cell after switching
        /// </summary>
        /// <param name="cellIdx">Cell index</param>
        private void UnlockCell(int cellIdx)
        {
            Interlocked.Exchange(ref Current.Cells[cellIdx].Lock, CellUnlocked);
        }

        /// <summary>
        /// Computes new speed for follower car
        /// </summary>
        /// <param name="dt">Delta time</param>
        /// <param name="followerIdx">Car index of follower</param>
        /// <param name="leaderIdx">Car index of leader; or -1 if there is no leader</param>
        private void CarFollowing(float dt, int followerIdx, int leaderIdx)
        {
            ref Car follower = ref Current.Cars[followerIdx];
            if (leaderIdx == -1) {
                follower.Speed = Math.Min(follower.Speed + dt, SpeedLimit);
                return;
            }

            ref Car leader = ref Current.Cars[leaderIdx];

            float gap = (leader.PositionInCell - leader.Size) - follower.PositionInCell;
            if (gap < 0) {
                follower.Speed = 0;
            } else {
                follower.Speed = Math.Min(Math.Min(follower.Speed + dt, gap), SpeedLimit);
            }

            if (follower.Speed >= 1) {
                float rand = random[(uint)(randomSeed + followerIdx) % randomLength];
                if (rand < SlowdownProbability) {
                    follower.Speed -= 1;
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
        /// <param name="dt">Delta time</param>
        /// <param name="i">Generator index</param>
        private void SpawnCars(float dt, int i)
        {
            ref Generator generator = ref Current.Generators[i];

            if (generator.TimeLeft > 0) {
                generator.TimeLeft -= dt;
                return;
            }

            generator.TimeLeft = RandomExp(generator.ProbabilityLambda, randomSeed * (i + 1));

            int carSize = 1 + (int)(random[(uint)(randomSeed * (i + 1) * 2) % randomLength] * 6);

            int jo = (Current.Cars.Length / 2) + (10 * i);
            for (int j = 0; j < Current.Cars.Length; j++) {
                int carIdx = (jo + j) % Current.Cars.Length;
                ref Car car = ref Current.Cars[carIdx];

                if (Interlocked.CompareExchange(ref car.Position, generator.CellIndex, Cell.None) != Cell.None) {
                    // This car is already spawned
                    continue;
                }

                bool success = false;
                for (int k = 0; k < Current.CarsPerCell; k++) {
                    int idx = generator.CellIndex * Current.CarsPerCell + k;
                    if (Current.CellsToCar[idx] == Cell.None) {
                        Current.CellsToCar[idx] = carIdx;

                        if (k == 0) {
                            // Lane is empty
                            success = true;
                        } else {
                            ref Car leaderCar = ref Current.Cars[Current.CellsToCar[idx - 1]];
                            float gap = (leaderCar.PositionInCell - leaderCar.Size);
                            success = (gap >= 0);
                        }

                        if (success) {
                            car.Position = generator.CellIndex;
                            car.PositionInCell = 0;
                            car.Speed = 0;
                            car.Size = carSize;
                            car.AlreadyProcessed = 1;
                        } else {
                            // There is not enough space, spawn is not possible
                            // Do rollback
                            Current.CellsToCar[idx] = Cell.None;
                        }
                        break;
                    }
                }

                if (!success) {
                    // There is not enough space, spawn is not possible
                    // Do rollback
                    Interlocked.Exchange(ref car.Position, Cell.None);
                }

                break;
            }
        }

        /// <inheritdoc />
        public override bool CheckIntegrity()
        {
            ActiveCarsCount = 0;
            WaitingCarsCount = 0;

            for (int i = 0; i < Current.Cars.Length; i++) {
                ref Car car = ref Current.Cars[i];
                if (car.Position == Cell.None) {
                    continue;
                }

                ActiveCarsCount++;
            }

            for (int i = 0; i < Current.Junctions.Length; i++) {
                WaitingCarsCount += Current.Junctions[i].WaitingCount;
            }

            return true;
        }

        /// <inheritdoc />
        protected override void InnerLoadFromStream(BinaryReader r)
        {
            Current = new SimulationData();

            // Cells
            {
                int count = r.ReadInt32();
                Current.Cells = new Cell[count];
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

                    cell.Length = r.ReadSingle();

                    ref CellUi cellUi = ref Current.CellsUi[i];
                    cellUi.X = r.ReadInt32();
                    cellUi.Y = r.ReadInt32();
                }
            }

            Current.CarsPerCell = r.ReadInt32();

            Current.CellsToCar = new int[Current.Cells.Length * Current.CarsPerCell];
            for (int i = 0; i < Current.CellsToCar.Length; i++) {
                Current.CellsToCar[i] = r.ReadInt32();
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
                    car.PositionInCell = r.ReadSingle();
                    car.Speed = r.ReadSingle();
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

                    w.Write(cell.Length);

                    ref CellUi cellUi = ref Current.CellsUi[i];
                    w.Write(cellUi.X);
                    w.Write(cellUi.Y);
                }
            }

            w.Write(Current.CarsPerCell);

            for (int i = 0; i < Current.CellsToCar.Length; i++) {
                w.Write(Current.CellsToCar[i]);
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
                    w.Write(car.PositionInCell);
                    w.Write(car.Speed);
                    w.Write(car.Size);

                    ref CarUi carUi = ref Current.CarsUi[i];
                    w.Write(carUi.Color);
                }
            }
        }
    }
}
