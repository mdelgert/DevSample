using System;

namespace DevSample
{
    /// <summary>
    ///     Represents the entry point of the program. Prior total elapsed time: 4237765 milliseconds.
    ///     Optimized total elapsed time: 111801 milliseconds. Around 4100% performance increase.
    /// </summary>
    internal abstract class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        private static void Main()
        {
            // Invokes the SampleProcessor to perform work cycles.
            SampleProcessor.WorkCycles();

            // Pauses the console to keep it open for observation.
            Console.Read();
        }
    }
}