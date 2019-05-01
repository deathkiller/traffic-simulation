using System;
using System.Collections.Generic;
using NOpenCL;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Represents specific compiled OpenCL kernel
    /// </summary>
    public class OpenCLKernel
    {
        private OpenCLKernelSet kernelSet;
        private NOpenCL.Kernel kernel;

        private int currentArg;
        private List<OpenCLBuffer> boundBuffers;

#if ENABLE_PROFILING
        private static System.Diagnostics.Stopwatch profilingStopwatch;
#endif

        public OpenCLKernel(OpenCLKernelSet kernelSet, string kernelName)
        {
            this.kernelSet = kernelSet;

            if (!kernelSet.Kernels.TryGetValue(kernelName, out this.kernel)) {
                this.kernel = kernelSet.Program.CreateKernel(kernelName);
            }

            this.currentArg = 0;
            this.boundBuffers = new List<OpenCLBuffer>();

#if ENABLE_PROFILING
            if (profilingStopwatch == null) {
                profilingStopwatch = System.Diagnostics.Stopwatch.StartNew();
            } else {
                profilingStopwatch.Restart();
            }
#endif
        }

        /// <summary>
        /// Creates and binds buffer to this kernel before execution
        /// </summary>
        /// <param name="ptr">Pointer to memory</param>
        /// <param name="size">Size of memory</param>
        /// <param name="readOnly">Bind as read-only</param>
        /// <returns>Current instance of kernel</returns>
        public unsafe OpenCLKernel BindBuffer(void* ptr, int size, bool readOnly)
        {
            try {
                NOpenCL.Buffer buffer = kernelSet.OwnerDevice.Context.CreateBuffer(
                    MemoryFlags.UseHostPointer | (readOnly ? MemoryFlags.ReadOnly : MemoryFlags.ReadWrite),
                    size,
                    new IntPtr(ptr));

                boundBuffers.Add(new OpenCLBuffer {
                    Ptr = new IntPtr(ptr),
                    Buffer = buffer,
                    OwnerDevice = kernelSet.OwnerDevice
                });

                kernel.Arguments[currentArg].SetValue(buffer);
                currentArg++;

                return this;
            } catch (Exception ex) {
                throw new OpenCLException("Cannot create buffer. Buffer size is probably too high.", ex);
            }
        }

        /// <summary>
        /// Binds buffer to this kernel before execution
        /// </summary>
        /// <param name="buffer">Buffer</param>
        /// <returns>Current instance of kernel</returns>
        public unsafe OpenCLKernel BindBuffer(OpenCLBuffer buffer)
        {
            try {
                //boundBuffers.Add(buffer);

                kernel.Arguments[currentArg].SetValue(buffer.Buffer);
                currentArg++;

                return this;
            } catch (Exception ex) {
                throw new OpenCLException("Cannot create buffer. Buffer size is probably too high.", ex);
            }
        }

        /// <summary>
        /// Binds single (int) value to this kernel before execution
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Current instance of kernel</returns>
        public OpenCLKernel BindValue(int value)
        {
            kernel.Arguments[currentArg].SetValue(value);
            currentArg++;

            return this;
        }

        /// <summary>
        /// Binds single (float) value to this kernel before execution
        /// </summary>
        /// <param name="value">Value</param>
        /// <returns>Current instance of kernel</returns>
        public unsafe OpenCLKernel BindValue(float value)
        {
            int rawValue = *(int*)&value;

            kernel.Arguments[currentArg].SetValue(rawValue);
            currentArg++;

            return this;
        }

        /// <summary>
        /// Binds single (int) value by argument index to this kernel before execution
        /// </summary>
        /// <param name="index">Argument index</param>
        /// <param name="value">Value</param>
        /// <returns>Current instance of kernel</returns>
        public OpenCLKernel BindValueByIndex(int index, int value)
        {
            kernel.Arguments[index].SetValue(value);

            return this;
        }

        /// <summary>
        /// Binds single (float) value by argument index to this kernel before execution
        /// </summary>
        /// <param name="index">Argument index</param>
        /// <param name="value">Value</param>
        /// <returns>Current instance of kernel</returns>
        public unsafe OpenCLKernel BindValueByIndex(int index, float value)
        {
            int rawValue = *(int*)&value;

            kernel.Arguments[index].SetValue(rawValue);

            return this;
        }

        /// <summary>
        /// Executes kernel
        /// </summary>
        /// <param name="n">Work size</param>
        /// <returns>Current instance of kernel</returns>
        public OpenCLKernel Run(int n)
        {
#if ENABLE_PROFILING
            Console.WriteLine("Kernel \"" + kernel.FunctionName + "\" binding took " + profilingStopwatch.ElapsedMilliseconds + " ms");
            profilingStopwatch.Restart();
#endif

            NOpenCL.CommandQueue commandQueue = kernelSet.OwnerDevice.CommandQueue;

            using (Event perfEvent = commandQueue.EnqueueNDRangeKernel(kernel, new[] { (IntPtr)n }, null)) {
#if ENABLE_PROFILING
                Event.WaitAll(perfEvent);

                ulong queuedToEnd = (perfEvent.CommandEndTime - perfEvent.CommandQueuedTime) / 1000000;
                ulong startToEnd = (perfEvent.CommandEndTime - perfEvent.CommandStartTime) / 1000000;

                Console.WriteLine("Kernel \"" + kernel.FunctionName + "\" execution took " + startToEnd + " ms (" + queuedToEnd + " ms)");
#endif
            }

#if ENABLE_PROFILING
            Console.WriteLine("Kernel \"" + kernel.FunctionName + "\" enqueue took " + profilingStopwatch.ElapsedMilliseconds + " ms");
            profilingStopwatch.Restart();
#endif

            return this;
        }

        /// <summary>
        /// Cleans up resources after execution
        /// </summary>
        public void Finish()
        {
            NOpenCL.CommandQueue commandQueue = kernelSet.OwnerDevice.CommandQueue;

            commandQueue.Finish();

            // Dispose all bound buffers
            for (int i = 0; i < boundBuffers.Count; i++) {
                if (boundBuffers[i].HasOwnership) {
                    continue;
                }

                boundBuffers[i].Synchronize();
                boundBuffers[i].Dispose();
            }

            boundBuffers.Clear();

            currentArg = 0;
        }
    }
}