using System;
using System.Collections.Generic;

namespace TrafficSimulation.Simulations.CarFollowing
{
    partial class CarFollowingSim
    {
        private class CellRef
        {
            public int T1, T2, T3, T4, T5;

            public int JunctionIndex;
            public int NearestJunctionIndex;

            public int X, Y;

            public float Length;
        }

        private struct CarSortHelperStruct
        {
            public int CarIndex;
            public float PositionInCell;
        }

        /// <inheritdoc />
        public override void GenerateNew(int distance, int junctionsX, int junctionsY, int carCount, int maxCarCount, float generatorProbability, int? randomSeed = null)
        {
            Random r;
            if (randomSeed == null) {
                r = new Random();
            } else {
                r = new Random(randomSeed.Value);
            }

            if (maxCarCount < carCount) {
                maxCarCount = carCount;
            }

            List<CellRef> cells = new List<CellRef>();

            // Create junctions
            Junction[] junctions = new Junction[junctionsX * junctionsY];

            for (int y = 0; y < junctionsY; y++) {
                for (int x = 0; x < junctionsX; x++) {
                    int i = x + y * junctionsX;

                    ref Junction j = ref junctions[i];

                    j.CellIndex = cells.Count;

                    CellRef c = new CellRef {
                        T1 = Cell.None, // Left
                        T2 = Cell.None, // Top
                        T3 = Cell.None, // Right
                        T4 = Cell.None, // Bottom
                        T5 = Cell.None, // Unused

                        JunctionIndex = i,
                        NearestJunctionIndex = Cell.None,

                        X = x * distance,
                        Y = y * distance
                    };

                    if (x > 0) {
                        int i2 = (x - 1) + y * junctionsX;
                        ref Junction j2 = ref junctions[i2];
                        c.T1 = i2;

                        CellRef c2 = cells[j2.CellIndex];
                        c2.T3 = i;
                    }

                    if (y > 0) {
                        int i2 = x + (y - 1) * junctionsX;
                        ref Junction j2 = ref junctions[i2];
                        c.T2 = i2;

                        CellRef c2 = cells[j2.CellIndex];
                        c2.T4 = i;
                    }

                    cells.Add(c);
                }
            }

            // Create lanes
            for (int i = 0; i < junctions.Length; i++) {
                ref Junction j = ref junctions[i];
                CellRef c = cells[j.CellIndex];

                CreateLane(ref j, ref c.T1, cells, distance, 0, -1); // Left
                CreateLane(ref j, ref c.T2, cells, distance, 1, 0); // Top
                CreateLane(ref j, ref c.T3, cells, distance, 0, 1); // Right
                CreateLane(ref j, ref c.T4, cells, distance, -1, 0); // Bottom
                CreateLane(ref j, ref c.T5, cells, distance, 0, 0); // Unused
            }

            // Create generators and terminators
            List<Generator> generators = new List<Generator>();
            for (int i = 0; i < junctions.Length; i++) {
                ref Junction j = ref junctions[i];
                CellRef c = cells[j.CellIndex];

                CreateGeneratorsAndTerminators(generators, ref j, i, ref c.T1, cells, distance, -1, 0, r, generatorProbability); // Left
                CreateGeneratorsAndTerminators(generators, ref j, i, ref c.T2, cells, distance, 0, -1, r, generatorProbability); // Top
                CreateGeneratorsAndTerminators(generators, ref j, i, ref c.T3, cells, distance, 1, 0, r, generatorProbability); // Right
                CreateGeneratorsAndTerminators(generators, ref j, i, ref c.T4, cells, distance, 0, 1, r, generatorProbability); // Bottom
                CreateGeneratorsAndTerminators(generators, ref j, i, ref c.T5, cells, distance, 0, 0, r, generatorProbability); // Unused
            }

            // Create cars
            int[] cellsToCar;
            Car[] cars;
            CarUi[] carsUi;
            CreateCars(cells, distance, carCount, maxCarCount, r, out cellsToCar, out cars, out carsUi);

            // Sort cars in every traffic lane, so first (leading) car in on index 0
            SortCarsInCell(cells, cellsToCar, cars, distance);

            Current = new SimulationData();
            Current.Junctions = junctions;
            Current.Generators = generators.ToArray();
            Current.Cars = cars;
            Current.CarsUi = carsUi;
            Current.CellsToCar = cellsToCar;
            Current.CarsPerCell = distance;

            // Create initial state
            Current.Cells = new Cell[cells.Count];
            Current.CellsUi = new CellUi[cells.Count];
            for (int i = 0; i < cells.Count; i++) {
                CellRef src = cells[i];
                ref Cell dst = ref Current.Cells[i];

                dst.T1 = src.T1;
                dst.T2 = src.T2;
                dst.T3 = src.T3;
                dst.T4 = src.T4;
                dst.T5 = src.T5;

                double p1 = (dst.T1 == Cell.None ? 0 : r.NextDouble());
                double p2 = (dst.T2 == Cell.None ? 0 : r.NextDouble());
                double p3 = (dst.T3 == Cell.None ? 0 : r.NextDouble());
                double p4 = (dst.T4 == Cell.None ? 0 : r.NextDouble());
                double p5 = (dst.T5 == Cell.None ? 0 : r.NextDouble());
                double pTotal = (p1 + p2 + p3 + p4 + p5);
                if (pTotal > 0) {
                    dst.P1 = (float)(p1 / pTotal);
                    dst.P2 = (float)(p2 / pTotal);
                    dst.P3 = (float)(p3 / pTotal);
                    dst.P4 = (float)(p4 / pTotal);
                    dst.P5 = (float)(p5 / pTotal);
                } else {
                    dst.P1 = dst.P2 = dst.P3 = dst.P4 = dst.P5 = 0;
                }

                dst.JunctionIndex = src.JunctionIndex;
                dst.NearestJunctionIndex = src.NearestJunctionIndex;
                dst.Length = src.Length;

                ref CellUi dstUi = ref Current.CellsUi[i];
                dstUi.X = src.X;
                dstUi.Y = src.Y;
            }

            LastTimeTotal = TimeSpan.Zero;
            LastTimeCars = TimeSpan.Zero;
            LastTimeGenerators = TimeSpan.Zero;
            ActiveCarsCount = 0;
            WaitingCarsCount = 0;
        }

        /// <summary>
        /// Creates cars in generated simulation
        /// </summary>
        /// <param name="cells">Cells</param>
        /// <param name="distance">Distance between junctions</param>
        /// <param name="carCount">Initial number of cars</param>
        /// <param name="maxCarCount">Max. number of cars</param>
        /// <param name="r">Random generator</param>
        /// <param name="cellsToCar">Generated cells to car</param>
        /// <param name="cars">Generated cars</param>
        /// <param name="carsUi">UI struct for generated cars</param>
        private static void CreateCars(List<CellRef> cells, int distance, int carCount, int maxCarCount, Random r, out int[] cellsToCar, out Car[] cars, out CarUi[] carsUi)
        {
            cellsToCar = new int[distance * cells.Count];
            for (int j = 0; j < cellsToCar.Length; j++) {
                cellsToCar[j] = Cell.None;
            }

            cars = new Car[maxCarCount];
            carsUi = new CarUi[maxCarCount];

            int i = 0;
            for (; i < carCount; i++) {
                int carSize = r.Next(1, 6 + 1);

                bool wasPlaced = false;

                for (int tryCurrent = 0; tryCurrent < 1000; tryCurrent++) {
                    int cellIndex = r.Next(0, cells.Count);
                    CellRef c = cells[cellIndex];
                    if (c.Length == 0) {
                        continue;
                    }

                    float positionInCell = r.Next(carSize, (int)c.Length);

                    for (int j = 0; j < distance; j++) {
                        if (cellsToCar[cellIndex * distance + j] == Cell.None) {
                            cellsToCar[cellIndex * distance + j] = i;

                            ref Car car = ref cars[i];
                            car.Position = cellIndex;
                            car.PositionInCell = positionInCell;
                            car.Speed = 0;
                            car.Size = carSize;

                            ref CarUi carUi = ref carsUi[i];
                            carUi.Color = r.Next(0, 10);

                            wasPlaced = true;
                            break;
                        }
                    }

                    if (wasPlaced) {
                        break;
                    }
                }

                if (!wasPlaced) {
                    break;
                }
            }

            // Initialize non-spawned cars
            for (; i < maxCarCount; i++) {
                ref Car car = ref cars[i];
                car.Position = Cell.None;
                car.PositionInCell = 0;
                car.Speed = 0;
                car.Size = 0;

                ref CarUi carUi = ref carsUi[i];
                carUi.Color = r.Next(0, 10);
            }
        }

        /// <summary>
        /// Sorts cars in every traffic lane, so first (leading) car in on index 0
        /// </summary>
        /// <param name="cells">Cells</param>
        /// <param name="cellsToCar">Cells to car</param>
        /// <param name="cars">Cars</param>
        /// <param name="distance">Distance between junctions</param>
        private static void SortCarsInCell(List<CellRef> cells, int[] cellsToCar, Car[] cars, int distance)
        {
            CarSortHelperStruct[] sortHelper = new CarSortHelperStruct[distance];
            for (int i = 0; i < cells.Count; i++) {
                for (int j = 0; j < distance; j++) {
                    int idx = i * distance + j;
                    if (cellsToCar[idx] == Cell.None) {
                        sortHelper[j].CarIndex = Cell.None;
                        sortHelper[j].PositionInCell = float.MinValue;
                    } else {
                        sortHelper[j].CarIndex = cellsToCar[idx];
                        sortHelper[j].PositionInCell = cars[cellsToCar[idx]].PositionInCell;
                    }
                }

                Array.Sort(sortHelper, (a, b) => b.PositionInCell.CompareTo(a.PositionInCell));

                for (int j = 0; j < distance; j++) {
                    int idx = i * distance + j;
                    cellsToCar[idx] = sortHelper[j].CarIndex;
                }
            }
        }

        /// <summary>
        /// Creates new traffic lane between junctions
        /// </summary>
        /// <param name="j">Source junction</param>
        /// <param name="target">Target junction cell index</param>
        /// <param name="cells">List of cells</param>
        /// <param name="distance">Distance between junctions</param>
        /// <param name="ox">X-axis offset for UI</param>
        /// <param name="oy">Y-axis offset for UI</param>
        private void CreateLane(ref Junction j, ref int target, List<CellRef> cells, int distance, int ox, int oy)
        {
            if (target == Cell.None) {
                return;
            }

            int realTarget = target;

            CellRef c1 = cells[j.CellIndex]; // Source junction
            CellRef c2 = cells[realTarget]; // Destination junction

            // Source cell
            CellRef cFrom = new CellRef {
                T1 = cells.Count + 1, // Left
                T2 = Cell.None, // Top
                T3 = Cell.None, // Right
                T4 = Cell.None, // Bottom
                T5 = Cell.None, // Unused

                JunctionIndex = Cell.None,
                NearestJunctionIndex = c2.JunctionIndex,

                X = c1.X + ox,
                Y = c1.Y + oy,

                Length = distance
            };

            target = cells.Count;

            cells.Add(cFrom);

            // Target cell
            CellRef cTo = new CellRef {
                T1 = realTarget, // Left
                T2 = Cell.None, // Top
                T3 = Cell.None, // Right
                T4 = Cell.None, // Bottom
                T5 = Cell.None, // Unused

                JunctionIndex = Cell.None,
                NearestJunctionIndex = c2.JunctionIndex,

                X = c2.X + ox,
                Y = c2.Y + oy
            };

            cells.Add(cTo);
        }

        /// <summary>
        /// Creates generators and terminators on borders of traffic network
        /// </summary>
        /// <param name="generators">List of generators</param>
        /// <param name="j">Current junction</param>
        /// <param name="junctionIndex">Current junction index</param>
        /// <param name="target">Target cell index</param>
        /// <param name="cells">List of cells</param>
        /// <param name="distance">Distance between junctions</param>
        /// <param name="ox">X-axis offset for UI</param>
        /// <param name="oy">Y-axis offset for UI</param>
        /// <param name="r">Random generator</param>
        /// <param name="generatorProbabilityLambda">Lambda parameter for generator</param>
        private void CreateGeneratorsAndTerminators(List<Generator> generators, ref Junction j, int junctionIndex,
            ref int target, List<CellRef> cells, int distance, int ox, int oy, Random r, float generatorProbabilityLambda)
        {
            if (target != Cell.None) {
                return;
            }

            CellRef c1 = cells[j.CellIndex]; // Source junction

            // Lane to terminator
            {
                // Source cell
                CellRef cFrom = new CellRef {
                    T1 = cells.Count + 1, // Left
                    T2 = Cell.None, // Top
                    T3 = Cell.None, // Right
                    T4 = Cell.None, // Bottom
                    T5 = Cell.None, // Unused

                    JunctionIndex = Cell.None,
                    NearestJunctionIndex = Cell.None,

                    X = c1.X + (ox == 0 ? (oy < 0 ? 1 : -1) : 0),
                    Y = c1.Y + (oy == 0 ? (ox > 0 ? 1 : -1) : 0),

                    Length = distance
                };

                target = cells.Count;


                cells.Add(cFrom);

                // Target cell
                CellRef cTo = new CellRef {
                    T1 = Cell.None, // Left
                    T2 = Cell.None, // Top
                    T3 = Cell.None, // Right
                    T4 = Cell.None, // Bottom
                    T5 = Cell.None, // Unused

                    JunctionIndex = Cell.Terminator,
                    NearestJunctionIndex = Cell.None,

                    X = c1.X + ox * distance + (ox == 0 ? (oy < 0 ? 1 : -1) : 0),
                    Y = c1.Y + oy * distance + (oy == 0 ? (ox > 0 ? 1 : -1) : 0)
                };

                cells.Add(cTo);
            }

            // Lane from generator
            {
                // Source cell
                CellRef cFrom = new CellRef {
                    T1 = cells.Count + 1, // Left
                    T2 = Cell.None, // Top
                    T3 = Cell.None, // Right
                    T4 = Cell.None, // Bottom
                    T5 = Cell.None, // Unused

                    JunctionIndex = Cell.None,
                    NearestJunctionIndex = junctionIndex,

                    X = c1.X + ox * distance + (ox == 0 ? (oy > 0 ? 1 : -1) : 0),
                    Y = c1.Y + oy * distance + (oy == 0 ? (ox < 0 ? 1 : -1) : 0),

                    Length = distance
                };

                generators.Add(new Generator {
                    CellIndex = cells.Count,
                    ProbabilityLambda = generatorProbabilityLambda
                });

                cells.Add(cFrom);

                // Target cell
                CellRef cTo = new CellRef {
                    T1 = j.CellIndex, // Left
                    T2 = Cell.None, // Top
                    T3 = Cell.None, // Right
                    T4 = Cell.None, // Bottom
                    T5 = Cell.None, // Unused

                    JunctionIndex = Cell.None,
                    NearestJunctionIndex = junctionIndex,

                    X = c1.X + (ox == 0 ? (oy > 0 ? 1 : -1) : 0),
                    Y = c1.Y + (oy == 0 ? (ox < 0 ? 1 : -1) : 0)
                };

                cells.Add(cTo);
            }
        }
    }
}
