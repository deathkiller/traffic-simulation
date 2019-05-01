using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TrafficSimulation.Utils.Tests
{
    [TestClass]
    public class OpenCLDispatcherTests
    {
        private const string ProgramSource1 = @"
void kernel compute_add(global int* a, global int* b, global int* result)
{
    int i = get_global_id(0);
    result[i] = a[i] + b[i];
}
";

        private const string ProgramSource2 = @"
void kernel compute_multiply_add(global int* a, global int* b, const int c, global int* result)
{
    int i = get_global_id(0);
    result[i] = a[i] + b[i] * c;
}
";

        [TestMethod]
        public void ReferenceNotOpenCL()
        {
            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 1);

            OpenCLDevice device = devices[0];

            Assert.IsNotNull(device.Name);
            Assert.IsNotNull(device.ShortName);
            Assert.IsNull(device.InnerDevice);
        }

        [TestMethod]
        public void Devices()
        {
            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);

            for (int i = 1; i < devices.Count; i++) {
                OpenCLDevice device = devices[i];

                Assert.IsNotNull(device.Name);
                Assert.IsNotNull(device.ShortName);
                Assert.IsNotNull(device.InnerDevice);
            }
        }

        [TestMethod]
        public void DisposeDevices()
        {
            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);

            for (int i = 0; i < devices.Count; i++) {
                OpenCLDevice device = devices[i];

                Assert.IsFalse(device.IsDisposed);
            }

            dispatcher.DisposeDevices();

            for (int i = 1; i < devices.Count; i++) {
                OpenCLDevice device = devices[i];

                Assert.IsTrue(device.IsDisposed);
            }
        }

        [TestMethod]
        public void KernelCompile()
        {
            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 2);

            var kernelSet = dispatcher.Compile(devices[1], "compute_add", name => {
                return ProgramSource1;
            });

            Assert.IsFalse(kernelSet.IsDisposed);

            Assert.IsNotNull(kernelSet.Kernels);
            Assert.AreEqual(devices[1], kernelSet.OwnerDevice);
            Assert.IsNotNull(kernelSet.Program);
        }

        [TestMethod]
        public void KernelDispose1()
        {
            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 2);

            var kernelSet = dispatcher.Compile(devices[1], "compute_add", name => {
                return ProgramSource1;
            });

            kernelSet.Dispose();

            Assert.IsTrue(kernelSet.IsDisposed);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException), "Device was disposed.")]
        public unsafe void KernelDispose2()
        {
            const int Length = 10;

            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 2);

            var kernelSet = dispatcher.Compile(devices[1], "compute_add", name => {
                return ProgramSource1;
            });

            kernelSet.Dispose();

            int[] a = new int[Length];
            int[] b = new int[Length];
            int[] result = new int[Length];

            for (int i = 0; i < Length; i++) {
                a[i] = i;
                b[i] = i * 10;
            }

            fixed (int* a_ptr = a)
            fixed (int* b_ptr = b)
            fixed (int* result_ptr = result) {
                kernelSet["compute_add"]
                    .BindBuffer(a_ptr, sizeof(int) * Length, true)
                    .BindBuffer(b_ptr, sizeof(int) * Length, true)
                    .BindBuffer(result_ptr, sizeof(int) * Length, false)
                    .Run(Length)
                    .Finish();
            }
        }

        [TestMethod]
        public unsafe void KernelRun()
        {
            const int Length = 10;

            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 2);

            var kernelSet = dispatcher.Compile(devices[1], "compute_add", name => {
                return ProgramSource1;
            });

            int[] a = new int[Length];
            int[] b = new int[Length];
            int[] result = new int[Length];

            for (int i = 0; i < Length; i++) {
                a[i] = i;
                b[i] = i * 10;
            }

            fixed (int* a_ptr = a)
            fixed (int* b_ptr = b)
            fixed (int* result_ptr = result) {
                kernelSet["compute_add"]
                    .BindBuffer(a_ptr, sizeof(int) * Length, true)
                    .BindBuffer(b_ptr, sizeof(int) * Length, true)
                    .BindBuffer(result_ptr, sizeof(int) * Length, false)
                    .Run(Length)
                    .Finish();
            }

            for (int i = 0; i < Length; i++) {
                int expected = a[i] + b[i];
                Assert.AreEqual(expected, result[i]);
            }
        }

        [TestMethod]
        public unsafe void KernelRun2()
        {
            const int Length = 10;

            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 2);

            var kernelSet = dispatcher.Compile(devices[1], "compute_add", name => {
                return ProgramSource1;
            });

            int[] a = new int[Length];
            int[] b = new int[Length];
            int[] result = new int[Length];

            for (int i = 0; i < Length; i++) {
                a[i] = i;
                b[i] = i * 10;
            }

            fixed (int* a_ptr = a)
            fixed (int* b_ptr = b)
            fixed (int* result_ptr = result) {

                using (var a_buffer = dispatcher.CreateBuffer(devices[1], a_ptr, sizeof(int) * Length, true))
                using (var b_buffer = dispatcher.CreateBuffer(devices[1], b_ptr, sizeof(int) * Length, true))
                using (var result_buffer = dispatcher.CreateBuffer(devices[1], result_ptr, sizeof(int) * Length, false)) {

                    kernelSet["compute_add"]
                        .BindBuffer(a_buffer)
                        .BindBuffer(b_buffer)
                        .BindBuffer(result_buffer)
                        .Run(Length)
                        .Finish();
                }
            }

            for (int i = 0; i < Length; i++) {
                int expected = a[i] + b[i];
                Assert.AreEqual(expected, result[i]);
            }
        }

        [TestMethod]
        public unsafe void KernelRun3()
        {
            const int Length = 10;

            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 2);

            var kernelSet = dispatcher.Compile(devices[1], "compute_multiply_add", name => {
                return ProgramSource2;
            });

            int[] a = new int[Length];
            int[] b = new int[Length];
            int c = 4;
            int[] result = new int[Length];

            for (int i = 0; i < Length; i++) {
                a[i] = i * 3;
                b[i] = i * 7;
            }

            fixed (int* a_ptr = a)
            fixed (int* b_ptr = b)
            fixed (int* result_ptr = result) {

                kernelSet["compute_multiply_add"]
                    .BindBuffer(a_ptr, sizeof(int) * Length, true)
                    .BindBuffer(b_ptr, sizeof(int) * Length, true)
                    .BindValue(c)
                    .BindBuffer(result_ptr, sizeof(int) * Length, false)
                    .Run(Length)
                    .Finish();
            }

            for (int i = 0; i < Length; i++) {
                int expected = a[i] + b[i] * c;
                Assert.AreEqual(expected, result[i]);
            }
        }

        [TestMethod]
        public unsafe void KernelRun4()
        {
            const int Length = 10;

            OpenCLDispatcher dispatcher = new OpenCLDispatcher();

            var devices = dispatcher.Devices;

            Assert.IsNotNull(devices);
            Assert.IsTrue(devices.Count >= 2);

            var kernelSet = dispatcher.Compile(devices[1], "compute_multiply_add", name => {
                return ProgramSource2;
            });

            int[] a = new int[Length];
            int[] b = new int[Length];
            int c = 4;
            int[] result = new int[Length];

            for (int i = 0; i < Length; i++) {
                a[i] = i * 3;
                b[i] = i * 7;
            }

            fixed (int* a_ptr = a)
            fixed (int* b_ptr = b)
            fixed (int* result_ptr = result) {

                kernelSet["compute_multiply_add"]
                    .BindBuffer(a_ptr, sizeof(int) * Length, true)
                    .BindBuffer(b_ptr, sizeof(int) * Length, true)
                    .BindValue(0)
                    .BindBuffer(result_ptr, sizeof(int) * Length, false)
                    .BindValueByIndex(2, c)
                    .Run(Length)
                    .Finish();
            }

            for (int i = 0; i < Length; i++) {
                int expected = a[i] + b[i] * c;
                Assert.AreEqual(expected, result[i]);
            }
        }
    }
}
