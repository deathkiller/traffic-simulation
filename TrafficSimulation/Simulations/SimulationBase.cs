using System.IO;
using System.Text;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Simulations
{
    /// <summary>
    /// Base class for all traffic simulation types
    /// </summary>
    public abstract class SimulationBase : Disposable
    {
        private const ushort FileFormatVersion = 1;

        /// <summary>
        /// Gets current traffic simulation model type
        /// </summary>
        public abstract SimulationType CurrentType { get; }

        /// <summary>
        /// Gets or sets simulation run-time flags
        /// </summary>
        public abstract SimulationFlags Flags { get; set; }

        /// <summary>
        /// Checks if simulation is ready to execute
        /// </summary>
        public abstract bool IsReady { get; }

        /// <summary>
        /// Gets current simulation time-step
        /// </summary>
        public abstract long CurrentStep { get; protected set; }

        /// <summary>
        /// Generates new simulation with specified parameters
        /// </summary>
        /// <param name="distance">Distance between junctions</param>
        /// <param name="junctionsX">Number of junctions on X-axis</param>
        /// <param name="junctionsY">Number of junctions on Y-axis</param>
        /// <param name="carCount">Initial number of cars</param>
        /// <param name="maxCarCount">Max. number of cars</param>
        /// <param name="generatorProbability">Probability of car generation</param>
        /// <param name="randomSeed">Random seed (optimal)</param>
        public abstract void GenerateNew(int distance, int junctionsX, int junctionsY, int carCount, int maxCarCount, float generatorProbability, int? randomSeed = null);

        /// <summary>
        /// Executes one step on reference device
        /// </summary>
        public abstract void DoStepReference();

        /// <summary>
        /// Executes one step on OpenCL device
        /// </summary>
        /// <param name="dispatcher">OpenCL dispatcher</param>
        /// <param name="device">OpenCL device</param>
        public abstract void DoStepOpenCL(OpenCLDispatcher dispatcher, OpenCLDevice device);

        /// <summary>
        /// Executes multiple steps on OpenCL device
        /// </summary>
        /// <param name="dispatcher">OpenCL dispatcher</param>
        /// <param name="device">OpenCL device</param>
        /// <param name="steps">Number of steps</param>
        public abstract void DoBatchOpenCL(OpenCLDispatcher dispatcher, OpenCLDevice device, int steps);

        /// <summary>
        /// Checks integrity of current simulation state
        /// </summary>
        /// <returns>True if it's OK; false, otherwise</returns>
        public abstract bool CheckIntegrity();

        /// <summary>
        /// Loads simulation-specific state data from stream
        /// </summary>
        /// <param name="r">Reader</param>
        protected abstract void InnerLoadFromStream(BinaryReader r);

        /// <summary>
        /// Saves simulation-specific state data to stream
        /// </summary>
        /// <param name="w">Writer</param>
        protected abstract void InnerSaveToStream(BinaryWriter w);

        /// <summary>
        /// Loads simulation state data from stream
        /// </summary>
        /// <param name="s">Stream</param>
        /// <returns>New instance of simulation</returns>
        public static SimulationBase LoadFromStream(Stream s, int? randomSeed = null)
        {
            using (BinaryReader r = new BinaryReader(s, Encoding.UTF8, true)) {
                ushort version = r.ReadUInt16();
                if (version != FileFormatVersion) {
                    throw new InvalidDataException("Unknown file format");
                }

                SimulationBase sim;

                ushort simId = r.ReadUInt16();
                long currentStep = r.ReadInt64();

                switch (simId) {
                    case 0: {
                        sim = new CellBased.CellBasedSim(randomSeed);
                        break;
                    }
                    case 1: {
                        sim = new CarFollowing.CarFollowingSim(randomSeed);
                        break;
                    }

                    default:
                        throw new InvalidDataException("Unknown simulation type");
                }

                sim.InnerLoadFromStream(r);
                sim.CurrentStep = currentStep;
                return sim;
            }
        }

        /// <summary>
        /// Saves simulation state data to stream
        /// </summary>
        /// <param name="s">Target stream</param>
        /// <param name="sim">Current simulation instance</param>
        public static void SaveToStream(Stream s, SimulationBase sim)
        {
            using (BinaryWriter w = new BinaryWriter(s, Encoding.UTF8, true)) {
                w.Write((ushort)FileFormatVersion);

                ushort simId;
                switch (sim) {
                    case CellBased.CellBasedSim sim2: {
                        simId = 0;
                        break;
                    }
                    case CarFollowing.CarFollowingSim sim2: {
                        simId = 1;
                        break;
                    }

                    default:
                        throw new InvalidDataException("Unknown simulation type");
                }
                w.Write((ushort)simId);

                w.Write((long)sim.CurrentStep);

                sim.InnerSaveToStream(w);
            }
        }
    }
}
