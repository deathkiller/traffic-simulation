using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrafficSimulation.Simulations;
using TrafficSimulation.Simulations.CarFollowing;
using TrafficSimulation.Simulations.CellBased;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Tests
{
    /// <summary>
    /// Contains helper methods used in testing
    /// </summary>
    public static class TestUtils
    {
        public const int StepCountToCheckpoint = 1000;
        public const int RandomSeed = 123456;

        /// <summary>
        /// OpenCL Device index used for testing
        /// </summary>
        public const int OpenCLDeviceIndex = 1;

        /// <summary>
        /// Execute simulation with reference implementation to checkpoint and compare with expected state
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="simType">Simulation type</param>
        /// <param name="stateName">State name</param>
        public static void DoStepsReferenceToCheckpointAndAssert(SimulationBase sim, SimulationType simType, string stateName)
        {
            for (int i = 0; i < StepCountToCheckpoint; i++) {
                sim.DoStepReference();
            }

            AssertSimulationWithState(sim, simType, stateName);
        }

        /// <summary>
        /// Executes simulation with OpenCL implementation to checkpoint and compares with expected state
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="simType">Simulation type</param>
        /// <param name="stateName">State name</param>
        public static void DoStepsOpenCLToCheckpointAndAssert(SimulationBase sim, SimulationType simType, string stateName)
        {
            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            for (int i = 0; i < StepCountToCheckpoint; i++) {
                sim.DoStepOpenCL(dispatcher, device);
            }

            AssertSimulationWithState(sim, simType, stateName);
        }

        /// <summary>
        /// Executes simulation with OpenCL implementation in batch to checkpoint and compares with expected state
        /// </summary>
        /// <param name="sim">Simulation</param>
        /// <param name="simType">Simulation type</param>
        /// <param name="stateName">State name</param>
        public static void DoBatchOpenCLToCheckpointAndAssert(SimulationBase sim, SimulationType simType, string stateName)
        {
            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            sim.DoBatchOpenCL(dispatcher, device, StepCountToCheckpoint);

            AssertSimulationWithState(sim, simType, stateName);
        }

        /// <summary>
        /// Loads, saves and checks if it's equal
        /// </summary>
        /// <param name="stateName">State name</param>
        public static void LoadSaveAndAssert(string stateName)
        {
            MemoryStream streamExpected = new MemoryStream();
            MemoryStream streamCurrent = new MemoryStream();

            using (Stream streamTemp = OpenStateFromResources(stateName)) {
                streamTemp.CopyTo(streamExpected);
            }

            streamExpected.Position = 0;
            streamCurrent.Position = 0;

            SimulationBase sim = SimulationBase.LoadFromStream(streamExpected);

            SimulationBase.SaveToStream(streamCurrent, sim);

            streamExpected.Position = 0;
            streamCurrent.Position = 0;

            SHA1Managed hash = new SHA1Managed();
            byte[] hashExpected = hash.ComputeHash(streamExpected);
            byte[] hashCurrent = hash.ComputeHash(streamCurrent);

            AssertArraysEqual(hashExpected, hashCurrent);
        }

        /// <summary>
        /// Checks if current simulation state is the same as expected state
        /// </summary>
        /// <param name="simCurrent">Simulation</param>
        /// <param name="simType">Simulation type</param>
        /// <param name="stateName">State name</param>
        public static void AssertSimulationWithState(SimulationBase simCurrent, SimulationType simType, string stateName)
        {
            SimulationBase simExpected = SimulationBase.LoadFromStream(OpenStateFromResources(stateName), RandomSeed);

            Assert.AreEqual(simType, simExpected.CurrentType);
            Assert.AreEqual(simExpected.CurrentType, simCurrent.CurrentType);

            Assert.IsTrue(simExpected.IsReady);
            Assert.AreEqual(simExpected.IsReady, simCurrent.IsReady);

            Assert.AreEqual(simExpected.CurrentStep, simCurrent.CurrentStep);

            Assert.AreEqual(simExpected.GetType(), simCurrent.GetType());

            switch (simType)
            {
                case SimulationType.CellBased: {
                    CellBasedSim simExpected2 = simExpected as CellBasedSim;
                    Assert.IsNotNull(simExpected2);

                    CellBasedSim simCurrent2 = simCurrent as CellBasedSim;
                    Assert.IsNotNull(simCurrent2);

                    Assert.IsTrue(simCurrent2.CheckIntegrity());
                    break;
                }

                case SimulationType.CarFollowing: {
                    CarFollowingSim simExpected2 = simExpected as CarFollowingSim;
                    Assert.IsNotNull(simExpected2);

                    CarFollowingSim simCurrent2 = simCurrent as CarFollowingSim;
                    Assert.IsNotNull(simCurrent2);

                    Assert.AreEqual(simExpected2.Current.CarsPerCell, simCurrent2.Current.CarsPerCell);

                    Assert.IsTrue(simCurrent2.CheckIntegrity());
                    break;
                }

                default:
                    Assert.Fail("Unknown simulation type");
                    break;
            }
        }

        /// <summary>
        /// Loads state from assembly resources
        /// </summary>
        /// <param name="filename">Filename</param>
        /// <returns>File stream</returns>
        public static Stream OpenStateFromResources(string filename)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string[] resources = a.GetManifestResourceNames();
            for (int j = 0; j < resources.Length; j++) {
                if (resources[j].EndsWith(".States." + filename, StringComparison.Ordinal)) {
                    return a.GetManifestResourceStream(resources[j]);
                }
            }

            return null;
        }

        /// <summary>
        /// Initializes OpenCL dispatcher and device
        /// </summary>
        /// <param name="dispatcher">OpenCL dispatcher</param>
        /// <param name="device">OpenCL device</param>
        public static void GetOpenCLDispatcherAndDevice(out OpenCLDispatcher dispatcher, out OpenCLDevice device)
        {
            dispatcher = new OpenCLDispatcher();
            
            try {
                if (dispatcher.Devices.Count <= 1) {
                    Assert.Inconclusive("OpenCL device is not present in this computer.");
                    device = null;
                } else if (dispatcher.Devices.Count <= OpenCLDeviceIndex) {
                    device = dispatcher.Devices[dispatcher.Devices.Count - 1];
                } else {
                    device = dispatcher.Devices[OpenCLDeviceIndex];
                }
            } catch {
                // Cannot get list of available devices
                Assert.Inconclusive("Cannot get list of OpenCL devices in this computer.");
                device = null;
            }
        }

        public static void AssertArraysEqual<T>(T[] expected, T[] actual) where T : IEquatable<T>
        {
            Assert.AreEqual(expected.Length, actual.Length);

            for (int i = 0; i < expected.Length; i++) {
                Assert.IsTrue(expected[i].Equals(actual[i]));
            }
        }
    }
}