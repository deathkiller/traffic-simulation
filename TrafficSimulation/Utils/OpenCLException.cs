using System;

namespace TrafficSimulation.Utils
{
    /// <summary>
    /// Exception type used by OpenCLDispatcher
    /// </summary>
    public class OpenCLException : Exception
    {
        public OpenCLException(string message) : base(message)
        {
        }

        public OpenCLException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}