using System;
using System.Collections.Generic;

namespace TrafficSimulation.Simulations.CellBased
{
    partial class CellBasedSim
    {
        private class CellRef
        {
            public int T1, T2, T3, T4, T5;

            public int JunctionIndex;
            public int NearestJunctionIndex;

            public int X, Y;

            public int CarIndex;
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

            maxCarCount = Math.Max(maxCarCount, carCount);

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
                        Y = y * distance,

                        CarIndex = Cell.None
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
            Car[] cars = new Car[maxCarCount];
            CarUi[] carsUi = new CarUi[maxCarCount];
            {
                int i = 0;

                // Spawn cars
                for (; i < carCount; i++) {
                    int carSize = r.Next(1, 6 + 1);
                    int[] carCells = new int[carSize];

                    bool wasPlaced = false;

                    for (int tryCurrent = 0; tryCurrent < 1000; tryCurrent++) {
                        int cellIndex = r.Next(0, cells.Count);

                        wasPlaced = true;
                        for (int j = 0; j < carSize; j++) {
                            CellRef c = cells[cellIndex];
                            if (c.JunctionIndex != Cell.None || c.T1 == Cell.None || c.CarIndex != Cell.None) {
                                wasPlaced = false;
                                break;
                            }

                            carCells[j] = cellIndex;

                            cellIndex = c.T1;
                        }

                        if (wasPlaced) {
                            ref Car car = ref cars[i];
                            car.Position = carCells[0];
                            car.Speed = 0;
                            car.Size = carSize;

                            ref CarUi carUi = ref carsUi[i];
                            carUi.Color = r.Next(0, 10);

                            for (int j = 0; j < carSize; j++) {
                                CellRef c = cells[carCells[j]];
                                c.CarIndex = i;
                            }

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
                    car.Speed = 0;
                    car.Size = 0;

                    ref CarUi carUi = ref carsUi[i];
                    carUi.Color = r.Next(0, 10);
                }
            }

            Current = new SimulationData();
            Current.Junctions = junctions;
            Current.Generators = generators.ToArray();
            Current.Cars = cars;
            Current.CarsUi = carsUi;

            // Convert cells to structs
            Current.Cells = new Cell[cells.Count];
            Current.CellsToCar = new CellToCar[cells.Count];
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

                ref CellToCar dstToCar = ref Current.CellsToCar[i];
                dstToCar.CarIndex = src.CarIndex;

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
        /// Creates new traffic lane between junctions
        /// </summary>
        /// <param name="j">Source junction</param>
        /// <param name="target">Target junction cell index</param>
        /// <param name="cells">List of cells</param>
        /// <param name="distance">Distance between junctions (number of cells)</param>
        /// <param name="ox">X-axis offset for UI</param>
        /// <param name="oy">Y-axis offset for UI</param>
        private void CreateLane(ref Junction j, ref int target, List<CellRef> cells, int distance, int ox, int oy)
        {
            if (target == Cell.None) {
                return;
            }

            CellRef c1 = cells[j.CellIndex]; // Source junction
            CellRef c2 = cells[target]; // Destination junction

            CellRef[] laneCells = new CellRef[distance];

            for (int i = 0; i < laneCells.Length; i++) {
                CellRef c = new CellRef {
                    T1 = Cell.None, // Left
                    T2 = Cell.None, // Top
                    T3 = Cell.None, // Right
                    T4 = Cell.None, // Bottom
                    T5 = Cell.None, // Unused

                    JunctionIndex = Cell.None,
                    NearestJunctionIndex = c2.JunctionIndex,

                    X = c1.X + ((c2.X - c1.X) / distance * i) + ox,
                    Y = c1.Y + ((c2.Y - c1.Y) / distance * i) + oy,

                    CarIndex = Cell.None
                };

                laneCells[i] = c;

                if (i > 0) {
                    laneCells[i - 1].T1 = cells.Count + i;
                }
            }

            laneCells[laneCells.Length - 1].T1 = target;

            target = cells.Count;

            cells.AddRange(laneCells);
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
        /// <param name="generatorProbability">Lambda parameter for generator</param>
        private void CreateGeneratorsAndTerminators(List<Generator> generators, ref Junction j, int junctionIndex,
            ref int target, List<CellRef> cells, int distance, int ox, int oy, Random r, float generatorProbability)
        {
            if (target != Cell.None) {
                return;
            }

            CellRef c1 = cells[j.CellIndex]; // Source junction

            // Lane to terminator
            {
                CellRef[] laneCells = new CellRef[distance];

                for (int i = 0; i < laneCells.Length; i++) {
                    CellRef c = new CellRef {
                        T1 = Cell.None, // Left
                        T2 = Cell.None, // Top
                        T3 = Cell.None, // Right
                        T4 = Cell.None, // Bottom
                        T5 = Cell.None, // Unused

                        JunctionIndex = Cell.None,
                        NearestJunctionIndex = Cell.None,

                        X = c1.X + ox * (i + 1) + (ox == 0 ? (oy < 0 ? 1 : -1) : 0),
                        Y = c1.Y + oy * (i + 1) + (oy == 0 ? (ox > 0 ? 1 : -1) : 0),

                        CarIndex = Cell.None
                    };

                    laneCells[i] = c;

                    if (i > 0) {
                        laneCells[i - 1].T1 = cells.Count + i;
                    }
                }

                laneCells[laneCells.Length - 1].JunctionIndex = Cell.Terminator;

                target = cells.Count;

                cells.AddRange(laneCells);
            }

            // Lane from generator
            {
                CellRef[] laneCells = new CellRef[distance];

                for (int i = 0; i < laneCells.Length; i++) {
                    CellRef c = new CellRef {
                        T1 = Cell.None, // Left
                        T2 = Cell.None, // Top
                        T3 = Cell.None, // Right
                        T4 = Cell.None, // Bottom
                        T5 = Cell.None, // Unused

                        JunctionIndex = Cell.None,
                        NearestJunctionIndex = junctionIndex,

                        X = c1.X + ox * (laneCells.Length - i) + (ox == 0 ? (oy > 0 ? 1 : -1) : 0),
                        Y = c1.Y + oy * (laneCells.Length - i) + (oy == 0 ? (ox < 0 ? 1 : -1) : 0),

                        CarIndex = Cell.None
                    };

                    laneCells[i] = c;

                    if (i > 0) {
                        laneCells[i - 1].T1 = cells.Count + i;
                    }
                }

                laneCells[laneCells.Length - 1].T1 = j.CellIndex;

                generators.Add(new Generator {
                    CellIndex = cells.Count,
                    ProbabilityLambda = generatorProbability
                });

                cells.AddRange(laneCells);
            }
        }
    }
}
 