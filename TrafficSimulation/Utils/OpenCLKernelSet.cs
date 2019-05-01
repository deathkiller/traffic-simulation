using System.Collections.Generic;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Represents set of OpenCL kernels compiled for specific device
    /// </summary>
    public class OpenCLKernelSet : Disposable
    {
        /// <summary>
        /// OpenCL Program
        /// </summary>
        public NOpenCL.Program Program;
        /// <summary>
        /// All compiled cached kernels for this device
        /// </summary>
        public Dictionary<string, NOpenCL.Kernel> Kernels;
        /// <summary>
        /// OpenCL Device
        /// </summary>
        public OpenCLDevice OwnerDevice;

        /// <summary>
        /// Gets specific kernel from kernel set
        /// </summary>
        /// <param name="kernelName">Kernel name</param>
        /// <returns>Kernel</returns>
        public OpenCLKernel this[string kernelName]
        {
            get
            {
                return new OpenCLKernel(this, kernelName);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (Kernels != null) {
                foreach (var pair in Kernels) {
                    pair.Value.Dispose();
                }
                Kernels = null;
            }

            if (Program != null) {
                Program.Dispose();
                Program = null;
            }
        }
    }
}