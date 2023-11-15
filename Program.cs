using System;

//Prior total elapsed time: 4237765 milliseconds. Optimized total elapsed time: 111801 milliseconds. Around 4100% performance increase.

namespace DevSample
{
    internal abstract class Program
    {
        private static void Main()
        {
            SampleProcessor.WorkCycles();
            Console.Read();
        }
    }
}