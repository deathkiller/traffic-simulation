using System;
using System.Threading;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Implements basic IDisposable pattern
    /// </summary>
    [Serializable]
    public abstract class Disposable : IDisposable
    {
        private int isDisposed;

        public bool IsDisposed
        {
            get
            {
                return (Thread.VolatileRead(ref isDisposed) == 1);
            }
        }

        ~Disposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) == 0) {
                try {
                    Dispose(true);
                } finally {
                    GC.SuppressFinalize(this);
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
        }
    }
}