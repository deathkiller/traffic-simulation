using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using NOpenCL;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Provices access to OpenCL devices
    /// </summary>
    public class OpenCLDispatcher
    {
        private const bool UseRelaxedMath = true;

        private OpenCLDevice[] availableDevices;

        /// <summary>
        /// Gets list of available devices
        /// Device at index 0 is non-OpenCL reference device
        /// </summary>
        public IReadOnlyList<OpenCLDevice> Devices
        {
            get
            {
                if (availableDevices == null) {
                    List<OpenCLDevice> result = new List<OpenCLDevice> {
                        new OpenCLDevice {
                            Name = "Reference",
                            ShortName = "Reference",
                            InnerDevice = null
                        }
                    };

                    Platform[] platforms = Platform.GetPlatforms();

                    for (int i = 0; i < platforms.Length; i++) {
                        Device[] devices = platforms[i].GetDevices(DeviceType.All);

                        for (int j = 0; j < devices.Length; j++) {
                            string version = devices[j].Version.Trim();
                            if (version.StartsWith("OpenCL ")) {
                                int idx = version.IndexOf(' ', 8);
                                if (idx != -1) {
                                    version = version.Substring(7, idx - 7);
                                } else {
                                    version = version.Substring(7);
                                }
                            }

                            result.Add(new OpenCLDevice {
                                Name = devices[j].DeviceType.ToString().ToUpperInvariant() + " · " + devices[j].Name.Trim().Replace("(R)", "®").Replace("(C)", "©").Replace("(TM)", "™") + " · " + platforms[i].Name.Trim().Replace("(R)", "®").Replace("(C)", "©").Replace("(TM)", "™") + " · v" + version,
                                ShortName = devices[j].Name.Trim(),
                                InnerDevice = devices[j]
                            });
                        }
                    }

                    availableDevices = result.ToArray();
                }

                return availableDevices;
            }
        }

        /// <summary>
        /// Disposes all devices and release resources
        /// </summary>
        public void DisposeDevices() {
            if (availableDevices != null) {
                for (int i = 0; i < availableDevices.Length; i++) {
                    availableDevices[i].Dispose();
                }

                availableDevices = null;
            }
        }

        /// <summary>
        /// Creates buffer on target device from memory
        /// </summary>
        /// <param name="device">Target device</param>
        /// <param name="ptr">Pointer to memory</param>
        /// <param name="size">Size of memory</param>
        /// <param name="readOnly">Bind as read-only</param>
        /// <returns>Buffer</returns>
        public unsafe OpenCLBuffer CreateBuffer(OpenCLDevice device, void* ptr, int size, bool readOnly)
        {
            try {
                NOpenCL.Buffer buffer = device.Context.CreateBuffer(
                            MemoryFlags.UseHostPointer | (readOnly ? MemoryFlags.ReadOnly : MemoryFlags.ReadWrite),
                            size,
                            new IntPtr(ptr));

                return new OpenCLBuffer {
                    Ptr = new IntPtr(ptr),
                    Buffer = buffer,
                    OwnerDevice = device,
                    HasOwnership = true
                };
            } catch (Exception ex) {
                throw new OpenCLException("Cannot create buffer. Buffer size is probably too high.", ex);
            }
        }

        /// <summary>
        /// Compiles specified OpenCL program on specified device
        /// </summary>
        /// <param name="device">OpenCL device</param>
        /// <param name="programName">Program name</param>
        /// <param name="programReceiveCallback">Callback to receive program sources; or null to use default storage</param>
        /// <returns>Kernel set for program</returns>
        public OpenCLKernelSet Compile(OpenCLDevice device, string programName, Func<string, string> programReceiveCallback = null)
        {
            const string buildOptions = "-cl-fast-relaxed-math -cl-mad-enable -cl-no-signed-zeros";

            InitializeDevice(device);

            OpenCLKernelSet kernelSet;

            if (device.CachedKernels == null) {
                device.CachedKernels = new Dictionary<string, OpenCLKernelSet>();
            } else {
                if (device.CachedKernels.TryGetValue(programName, out kernelSet)) {
                    return kernelSet;
                }
            }

            string programSource;
            if (programReceiveCallback != null) {
                programSource = programReceiveCallback(programName);
            } else {
                programSource = LoadSourceFromResources(programName);
            }

            Program program = device.Context.CreateProgramWithSource(programSource);

            program.Build(UseRelaxedMath ? buildOptions : null);

#if ENABLE_PROFILING
            Console.WriteLine("Using kernel {0}...", programName);
            Console.WriteLine("OpenCL Build log: " + program.GetBuildLog(device.InnerDevice));
#endif

            kernelSet = new OpenCLKernelSet {
                Program = program,
                Kernels = new Dictionary<string, NOpenCL.Kernel>(),
                OwnerDevice = device
            };

            device.CachedKernels[programName] = kernelSet;
            return kernelSet;
        }

        /// <summary>
        /// Initialize specified OpenCL device
        /// </summary>
        /// <param name="device">Device</param>
        private void InitializeDevice(OpenCLDevice device)
        {
            if (device.Context != null) {
                return;
            }

            Context context = null;
            CommandQueue commandQueue = null;

            try {
                context = Context.Create(device.InnerDevice);

#if ENABLE_PROFILING
                commandQueue = context.CreateCommandQueue(device.InnerDevice, CommandQueueProperties.ProfilingEnable);
#else
                commandQueue = context.CreateCommandQueue(device.InnerDevice, 0);
#endif
            } catch (Exception ex) {
                if (commandQueue != null) {
                    commandQueue.Dispose();
                }

                if (context != null) {
                    context.Dispose();
                }

                throw new OpenCLException("Device is not available (" + ex.Message + ").", ex);
            }

            device.Context = context;
            device.CommandQueue = commandQueue;
        }

        /// <summary>
        /// Loads program source from assembly resources
        /// </summary>
        /// <param name="filename">Program filename</param>
        /// <returns>Program source</returns>
        private string LoadSourceFromResources(string filename)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            string[] resources = a.GetManifestResourceNames();
            for (int j = 0; j < resources.Length; j++) {
                if (resources[j].EndsWith(".Kernels." + filename, StringComparison.Ordinal)) {
                    using (Stream s = a.GetManifestResourceStream(resources[j]))
                    using (StreamReader r = new StreamReader(s, Encoding.UTF8)) {
                        return r.ReadToEnd();
                    }
                }
            }

            return null;
        }
    }
}
