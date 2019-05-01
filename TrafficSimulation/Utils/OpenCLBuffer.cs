using System;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Represents OpenCL memory buffer
    /// </summary>
    public class OpenCLBuffer : Disposable
    {
        /// <summary>
        /// Helper IDisposable struct for MapAsWritable()
        /// </summary>
        private struct BufferWriteAccessToken : IDisposable
        {
            private OpenCLBuffer buffer;

            public BufferWriteAccessToken(OpenCLBuffer buffer)
            {
                this.buffer = buffer;
                this.buffer.MapBufferToPointerWritable();
            }

            public void Dispose()
            {
                if (this.buffer != null) {
                    this.buffer.UnmapBufferToPointer();
                    this.buffer = null;
                }
            }
        }

        public IntPtr Ptr;
        public NOpenCL.Buffer Buffer;
        public OpenCLDevice OwnerDevice;
        public bool HasOwnership;

        protected override void Dispose(bool disposing)
        {
            if (Buffer != null) {
                if (HasOwnership) {
                    Synchronize();
                }

                Buffer.Dispose();
                Buffer = null;
                Ptr = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Copy memory from target device back to source pointer
        /// </summary>
        public void Synchronize()
        {
            NOpenCL.CommandQueue commandQueue = OwnerDevice.CommandQueue;

            IntPtr tmpPtr;
            using (commandQueue.EnqueueMapBuffer(Buffer, true, NOpenCL.MapFlags.Read, 0, (long)Buffer.Size, out tmpPtr)) {
                // Nothing to do...
            }

            if (Ptr != tmpPtr) {
                throw new InvalidOperationException("EnqueueMapBuffer() failed to return original pointer");
            }

            using (commandQueue.EnqueueUnmapMemObject(Buffer, tmpPtr)) {
                // Nothing to do...
            }
        }

        /// <summary>
        /// Map buffer as writable, copy memory from target device back to source pointer and when it's out of scope, copy changed memory to target device
        /// </summary>
        /// <returns></returns>
        public IDisposable MapAsWritable()
        {
            return new BufferWriteAccessToken(this);
        }

        /// <summary>
        /// Map buffer as writable, used internally by MapAsWritable()
        /// </summary>
        private void MapBufferToPointerWritable()
        {
            NOpenCL.CommandQueue commandQueue = OwnerDevice.CommandQueue;

            IntPtr tmpPtr;
            using (commandQueue.EnqueueMapBuffer(Buffer, true, NOpenCL.MapFlags.Read | NOpenCL.MapFlags.Write, 0, (long)Buffer.Size, out tmpPtr)) {
                // Nothing to do...
            }

            if (Ptr != tmpPtr) {
                throw new InvalidOperationException("EnqueueMapBuffer() failed to return original pointer");
            }
        }

        /// <summary>
        /// Unmap mapped buffer, used internally by MapAsWritable()
        /// </summary>
        private void UnmapBufferToPointer()
        {
            NOpenCL.CommandQueue commandQueue = OwnerDevice.CommandQueue;

            using (commandQueue.EnqueueUnmapMemObject(Buffer, Ptr)) {
                // Nothing to do...
            }
        }
    }
}