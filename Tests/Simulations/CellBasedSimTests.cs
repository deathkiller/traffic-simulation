using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrafficSimulation.Simulations.CellBased;
using TrafficSimulation.Tests;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Simulations.Tests
{
    [TestClass]
    public class CellBasedSimTests
    {
        [TestMethod]
        public void OneStepReference()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            sim.DoStepReference();

            Assert.AreEqual(SimulationType.CellBased, sim.CurrentType);
            Assert.IsTrue(sim.IsReady);
            Assert.AreEqual(1, sim.CurrentStep);

            Assert.AreEqual(3000, sim.Current.Cars.Length);
            Assert.AreEqual(3000, sim.Current.CarsUi.Length);
            Assert.AreEqual(15460, sim.Current.Cells.Length);
            Assert.AreEqual(15460, sim.Current.CellsToCar.Length);
            Assert.AreEqual(15460, sim.Current.CellsUi.Length);
            Assert.AreEqual(140, sim.Current.Generators.Length);
            Assert.AreEqual(100, sim.Current.Junctions.Length);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sim.ToString()));
        }

        [TestMethod]
        public void OneStepOpenCL()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            TestUtils.GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            sim.DoStepOpenCL(dispatcher, device);

            Assert.AreEqual(SimulationType.CellBased, sim.CurrentType);
            Assert.IsTrue(sim.IsReady);
            Assert.AreEqual(1, sim.CurrentStep);

            Assert.AreEqual(3000, sim.Current.Cars.Length);
            Assert.AreEqual(3000, sim.Current.CarsUi.Length);
            Assert.AreEqual(15460, sim.Current.Cells.Length);
            Assert.AreEqual(15460, sim.Current.CellsToCar.Length);
            Assert.AreEqual(15460, sim.Current.CellsUi.Length);
            Assert.AreEqual(140, sim.Current.Generators.Length);
            Assert.AreEqual(100, sim.Current.Junctions.Length);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sim.ToString()));
        }

        [TestMethod]
        public void DoStepsToCheckpointReference()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.DoStepsReferenceToCheckpointAndAssert(sim, SimulationType.CellBased, "CellBased.trs");
        }

        [TestMethod]
        public void DoStepsToCheckpointOpenCL()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.DoStepsOpenCLToCheckpointAndAssert(sim, SimulationType.CellBased, "CellBased.trs");
        }

        [TestMethod]
        public void DoBatchToCheckpointOpenCL()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.DoBatchOpenCLToCheckpointAndAssert(sim, SimulationType.CellBased, "CellBased.trs");
        }

        [TestMethod]
        public void GenerateNew()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.AssertSimulationWithState(sim, SimulationType.CellBased, "CellBasedGenerated.trs");
        }

        [TestMethod]
        public void CarMovementReference()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.Flags = SimulationFlags.NoSpawn;
            sim.GenerateNew(10, 2, 1, 1, 1, float.PositiveInfinity, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreEqual(0, sim.Current.CellsToCar[carPrev.Position].CarIndex);

            sim.DoStepReference();
            sim.DoStepReference();

            Car carNext = sim.Current.Cars[0];

            Assert.AreEqual(sim.Current.Cells[carPrev.Position].T1, carNext.Position);
            Assert.AreEqual(1f, carNext.Speed);

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position].CarIndex);
        }

        [TestMethod]
        public void CarMovementOpenCL()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.Flags = SimulationFlags.NoSpawn;
            sim.GenerateNew(10, 2, 1, 1, 1, float.PositiveInfinity, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreEqual(0, sim.Current.CellsToCar[carPrev.Position].CarIndex);

            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            TestUtils.GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            sim.DoStepOpenCL(dispatcher, device);
            sim.DoStepOpenCL(dispatcher, device);

            Car carNext = sim.Current.Cars[0];

            Assert.AreEqual(sim.Current.Cells[carPrev.Position].T1, carNext.Position);
            Assert.AreEqual(1f, carNext.Speed);

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position].CarIndex);
        }

        [TestMethod]
        public void CarSpawnReference()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(10, 1, 1, 0, 1, 0, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreEqual(Cell.None, carPrev.Position);

            sim.DoStepReference();

            Car carNext = sim.Current.Cars[0];

            Assert.AreNotEqual(Cell.None, carNext.Position);
            Assert.AreEqual(0f, carNext.Speed);

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position].CarIndex);
        }

        [TestMethod]
        public void CarSpawnOpenCL()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(10, 1, 1, 0, 1, 0, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreEqual(Cell.None, carPrev.Position);

            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            TestUtils.GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            sim.DoStepOpenCL(dispatcher, device);

            Car carNext = sim.Current.Cars[0];

            Assert.AreNotEqual(Cell.None, carNext.Position);
            Assert.AreEqual(0f, carNext.Speed);

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position].CarIndex);
        }

        [TestMethod]
        public void CarTerminationReference()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.Flags = SimulationFlags.NoSpawn;
            sim.GenerateNew(10, 1, 1, 1, 1, float.PositiveInfinity, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreNotEqual(Cell.None, carPrev.Position);

            for (int i = 0; i < 20; i++) {
                sim.DoStepReference();
            }

            Car carNext = sim.Current.Cars[0];

            Assert.AreEqual(Cell.None, carNext.Position);
        }

        [TestMethod]
        public void CarTerminationOpenCL()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.Flags = SimulationFlags.NoSpawn;
            sim.GenerateNew(10, 1, 1, 1, 1, float.PositiveInfinity, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreNotEqual(Cell.None, carPrev.Position);

            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            TestUtils.GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            for (int i = 0; i < 20; i++) {
                sim.DoStepOpenCL(dispatcher, device);
            }

            Car carNext = sim.Current.Cars[0];

            Assert.AreEqual(Cell.None, carNext.Position);
        }

        [TestMethod]
        public void LoadSave()
        {
            TestUtils.LoadSaveAndAssert("CellBased.trs");
        }

        [TestMethod]
        public void CheckIntegrity()
        {
            CellBasedSim sim = new CellBasedSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            sim.DoStepReference();

            bool result = sim.CheckIntegrity();

            Assert.IsTrue(result);
        }
    }
}