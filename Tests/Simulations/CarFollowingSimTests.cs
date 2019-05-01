using Microsoft.VisualStudio.TestTools.UnitTesting;
using TrafficSimulation.Simulations.CarFollowing;
using TrafficSimulation.Tests;
using TrafficSimulation.Utils;

namespace TrafficSimulation.Simulations.Tests
{
    [TestClass]
    public class CarFollowingSimTests
    {
        [TestMethod]
        public void OneStepReference()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            sim.DoStepReference();

            Assert.AreEqual(SimulationType.CarFollowing, sim.CurrentType);
            Assert.IsTrue(sim.IsReady);
            Assert.AreEqual(1, sim.CurrentStep);

            Assert.AreEqual(3000, sim.Current.Cars.Length);
            Assert.AreEqual(3000, sim.Current.CarsUi.Length);
            Assert.AreEqual(1380, sim.Current.Cells.Length);
            Assert.AreEqual(33120, sim.Current.CellsToCar.Length);
            Assert.AreEqual(1380, sim.Current.CellsUi.Length);
            Assert.AreEqual(140, sim.Current.Generators.Length);
            Assert.AreEqual(100, sim.Current.Junctions.Length);
            Assert.AreEqual(24, sim.Current.CarsPerCell);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sim.ToString()));
        }

        [TestMethod]
        public void OneStepOpenCL()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            TestUtils.GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            sim.DoStepOpenCL(dispatcher, device);

            Assert.AreEqual(SimulationType.CarFollowing, sim.CurrentType);
            Assert.IsTrue(sim.IsReady);
            Assert.AreEqual(1, sim.CurrentStep);

            Assert.AreEqual(3000, sim.Current.Cars.Length);
            Assert.AreEqual(3000, sim.Current.CarsUi.Length);
            Assert.AreEqual(1380, sim.Current.Cells.Length);
            Assert.AreEqual(33120, sim.Current.CellsToCar.Length);
            Assert.AreEqual(1380, sim.Current.CellsUi.Length);
            Assert.AreEqual(140, sim.Current.Generators.Length);
            Assert.AreEqual(100, sim.Current.Junctions.Length);
            Assert.AreEqual(24, sim.Current.CarsPerCell);

            Assert.IsFalse(string.IsNullOrWhiteSpace(sim.ToString()));
        }

        [TestMethod]
        public void DoStepsToCheckpointReference()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.DoStepsReferenceToCheckpointAndAssert(sim, SimulationType.CarFollowing, "CarFollowing.trs");
        }

        [TestMethod]
        public void DoStepsToCheckpointOpenCL()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.DoStepsOpenCLToCheckpointAndAssert(sim, SimulationType.CarFollowing, "CarFollowing.trs");
        }

        [TestMethod]
        public void DoBatchToCheckpointOpenCL()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.DoBatchOpenCLToCheckpointAndAssert(sim, SimulationType.CarFollowing, "CarFollowing.trs");
        }

        [TestMethod]
        public void GenerateNew()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            TestUtils.AssertSimulationWithState(sim, SimulationType.CarFollowing, "CarFollowingGenerated.trs");
        }

        [TestMethod]
        public void CarMovementReference()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.Flags = SimulationFlags.NoSpawn;
            sim.GenerateNew(10, 2, 1, 1, 1, float.PositiveInfinity, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreEqual(0, sim.Current.CellsToCar[carPrev.Position * sim.Current.CarsPerCell]);

            sim.DoStepReference();

            Car carNext = sim.Current.Cars[0];

            Assert.AreEqual(carPrev.Position, carNext.Position);
            Assert.AreEqual(1f, carNext.Speed);

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position * sim.Current.CarsPerCell]);
        }

        [TestMethod]
        public void CarMovementOpenCL()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.Flags = SimulationFlags.NoSpawn;
            sim.GenerateNew(10, 2, 1, 1, 1, float.PositiveInfinity, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreEqual(0, sim.Current.CellsToCar[carPrev.Position * sim.Current.CarsPerCell]);

            OpenCLDispatcher dispatcher;
            OpenCLDevice device;
            TestUtils.GetOpenCLDispatcherAndDevice(out dispatcher, out device);

            sim.DoStepOpenCL(dispatcher, device);

            Car carNext = sim.Current.Cars[0];

            Assert.AreEqual(carPrev.Position, carNext.Position);
            Assert.AreEqual(1f, carNext.Speed);

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position * sim.Current.CarsPerCell]);
        }

        [TestMethod]
        public void CarSpawnReference()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(10, 1, 1, 0, 1, 0, TestUtils.RandomSeed);

            Car carPrev = sim.Current.Cars[0];

            Assert.AreEqual(Cell.None, carPrev.Position);

            sim.DoStepReference();

            Car carNext = sim.Current.Cars[0];

            Assert.AreNotEqual(Cell.None, carNext.Position);
            Assert.AreEqual(0f, carNext.Speed);

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position * sim.Current.CarsPerCell]);
        }

        [TestMethod]
        public void CarSpawnOpenCL()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
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

            Assert.AreEqual(0, sim.Current.CellsToCar[carNext.Position * sim.Current.CarsPerCell]);
        }

        [TestMethod]
        public void CarTerminationReference()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
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
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
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
            TestUtils.LoadSaveAndAssert("CarFollowing.trs");
        }

        [TestMethod]
        public void CheckIntegrity()
        {
            CarFollowingSim sim = new CarFollowingSim(TestUtils.RandomSeed);
            sim.GenerateNew(24, 10, 10, 500, 3000, 0.1f, TestUtils.RandomSeed);

            sim.DoStepReference();

            bool result = sim.CheckIntegrity();

            Assert.IsTrue(result);
        }
    }
}