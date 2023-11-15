using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DevSample
{
    internal static class SampleProcessor
    {
        private static readonly int CyclesToRun;
        private static readonly int SamplesToLoad;
        private static readonly DateTime SampleStartDate;
        private static readonly TimeSpan SampleIncrement;

        static SampleProcessor()
        {
            // Note: these settings should not be modified
            CyclesToRun =
                Environment.ProcessorCount > 1
                    ? Environment.ProcessorCount / 2
                    : 1; // hopefully we have more than 1 core to work with, run cores/2 cycles with a max of 4 cycles
            CyclesToRun = CyclesToRun > 4 ? 4 : CyclesToRun;
            SamplesToLoad = 222222;
            SampleStartDate = new DateTime(1990, 1, 1, 1, 1, 1, 1);
            SampleIncrement = new TimeSpan(0, 5, 0);
        }

        public static void WorkCycles()
        {
            var totalMonitor = new Stopwatch();

            totalMonitor.Start();
            FileLogger.LogMessage(
                $"Starting Execution on a {Environment.ProcessorCount} core system. A total of {CyclesToRun} cycles will be run");

            Parallel.For(0, CyclesToRun, i => // 400% performance increase
            {
                try
                {
                    var cycleTimer = new Stopwatch();
                    var sampleGenerator = new SampleGenerator(SampleStartDate, SampleIncrement, SamplesToLoad);

                    FileLogger.LogMessage($"Cycle {i} Started Sample Load.");
                    cycleTimer.Start();
                    sampleGenerator.LoadSamples(SamplesToLoad);
                    cycleTimer.Stop();

                    var cycleElapsedTime = cycleTimer.Elapsed;

                    FileLogger.LogMessage(
                        $"Cycle {i} Finished Sample Load. Load Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");
                    FileLogger.LogMessage($"Cycle {i} Started Sample Validation.");
                    cycleTimer.Restart();
                    sampleGenerator.ValidateSamples();
                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;
                    FileLogger.LogMessage(
                        $"Cycle {i} Finished Sample Validation. Total Samples Validated: {sampleGenerator.SamplesValidated}. Validation Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");

                    var valueSum = sampleGenerator.Samples.Sum(s => s.Value);
                    // Complete: why do we only seem to get 7 digits of precision? The CEO wants to see at least 20!
                    FileLogger.LogMessage($"Cycle {i} Sum of All Samples: {valueSum:N}.");
                    FileLogger.LogMessage(
                        $"Cycle {i} Finished. Total Cycle Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");
                }
                catch (Exception ex)
                {
                    FileLogger.LogMessage($"Execution Failed!\n{ex}");
                }
            });

            totalMonitor.Stop();
            FileLogger.LogMessage("-----");
            FileLogger.LogMessage(
                $"Execution Finished. Total Elapsed Time: {totalMonitor.Elapsed.TotalMilliseconds:N} ms.");
        }
    }
}