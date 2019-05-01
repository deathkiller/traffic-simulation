using System.Collections.Generic;
using NOpenCL;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Represents OpenCL device
    /// </summary>
    public class OpenCLDevice : Disposable
    {
        public string Name;
        public string ShortName;
        
        /// <summary>
        /// OpenCL Device
        /// </summary>
        public Device InnerDevice;
        /// <summary>
        /// OpenCL Context
        /// </summary>
        public Context Context;
        /// <summary>
        /// OpenCL Command Queue
        /// </summary>
        public CommandQueue CommandQueue;
        /// <summary>
        /// All compiled cached programs (sets of kernels) for this device
        /// </summary>
        public Dictionary<string, OpenCLKernelSet> CachedKernels;

        public override string ToString()
        {
            return Name;
        }

        protected override void Dispose(bool disposing)
        {
            if (CachedKernels != null) {
                foreach (var pair in CachedKernels) {
                    pair.Value.Dispose();
                }
                CachedKernels = null;
            }

            if (CommandQueue != null) {
                CommandQueue.Dispose();
                CommandQueue = null;
            }

            if (Context != null) {
                Context.Dispose();
                Context = null;
            }

            if (InnerDevice != null) {
                InnerDevice.Dispose();
                InnerDevice = null;
            }
        }
    }
}