using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

//Prior total elapsed time: 4237765 milliseconds. Optimized total elapsed time: 111801 milliseconds. Around 4100% performance increase.

namespace DevSample
{
    internal abstract class Program
    {
        private static readonly string LogFile;
        private static readonly int CyclesToRun;
        private static readonly int SamplesToLoad;
        private static readonly DateTime SampleStartDate;
        private static readonly TimeSpan SampleIncrement;

        static Program()
        {
            // Note: these settings should not be modified
            LogFile = $"{ConfigurationManager.AppSettings["LogFilePath"]}\\{DateTime.Now:yyyyMMddHHmmss}_log.txt";
            CyclesToRun =
                Environment.ProcessorCount > 1
                    ? Environment.ProcessorCount / 2
                    : 1; // hopefully we have more than 1 core to work with, run cores/2 cycles with a max of 4 cycles
            CyclesToRun = CyclesToRun > 4 ? 4 : CyclesToRun;
            SamplesToLoad = 222222;
            SampleStartDate = new DateTime(1990, 1, 1, 1, 1, 1, 1);
            SampleIncrement = new TimeSpan(0, 5, 0);
        }

        private static void Main(string[] args)
        {
            var totalMonitor = new Stopwatch();
            
            totalMonitor.Start();
            LogMessage(
                $"Starting Execution on a {Environment.ProcessorCount} core system. A total of {CyclesToRun} cycles will be run");

            Parallel.For(0, CyclesToRun, i => // 400% performance increase
            {
                try
                {
                    var cycleTimer = new Stopwatch();
                    var sampleGenerator = new SampleGenerator(SampleStartDate, SampleIncrement, SamplesToLoad);

                    LogMessage($"Cycle {i} Started Sample Load.");
                    cycleTimer.Start();
                    sampleGenerator.LoadSamples(SamplesToLoad);
                    cycleTimer.Stop();
                    
                    var cycleElapsedTime = cycleTimer.Elapsed;
                    
                    LogMessage(
                        $"Cycle {i} Finished Sample Load. Load Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");
                    LogMessage($"Cycle {i} Started Sample Validation.");
                    cycleTimer.Restart();
                    sampleGenerator.ValidateSamples();
                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;
                    LogMessage(
                        $"Cycle {i} Finished Sample Validation. Total Samples Validated: {sampleGenerator.SamplesValidated}. Validation Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");

                    var valueSum = sampleGenerator.Samples.Sum(s => s.Value);
                    // Complete: why do we only seem to get 7 digits of precision? The CEO wants to see at least 20!
                    LogMessage($"Cycle {i} Sum of All Samples: {valueSum:N}.");
                    LogMessage($"Cycle {i} Finished. Total Cycle Time: {cycleElapsedTime.TotalMilliseconds:N} ms.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Execution Failed!\n{ex}");
                }
            });

            totalMonitor.Stop();
            LogMessage("-----");
            LogMessage($"Execution Finished. Total Elapsed Time: {totalMonitor.Elapsed.TotalMilliseconds:N} ms.");
            Console.Read();
        }

        private static void LogMessage(string message)
        {
            var logMessage = $"{DateTime.Now:HH:mm:ss.fffff} - {message}";
            Console.WriteLine(logMessage);
            //Don't wait for the logger to write to file for more performance.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LogToFileAsync(logMessage);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
        
        private static async Task LogToFileAsync(string message)
        {
            // Complete: implement this when someone complains about it not working... everything written to the console should
            // also be written to a log under C:\Temp. A new log with a unique file name should be created each time the application is run.
            const int maxRetries = 10;
            var retries = 0;

            while (retries < maxRetries)
                try
                {
                    // Append the log entry to the log file
                    File.AppendAllText(LogFile, message + Environment.NewLine);
                    return; // If successful, exit the loop
                }
                catch (IOException ex) when (IsFileLocked(ex))
                {
                    // If file is locked, wait a moment and retry
                    await Task.Delay(10);
                    retries++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error appending to log file: {ex.Message}");
                    return; // Exit the loop on any other exception
                }

            Console.WriteLine($"Failed to append to log file after {maxRetries} retries.");
        }

        // Helper method to check if the exception is due to a locked file
        private static bool IsFileLocked(Exception ex)
        {
            if (!(ex is IOException ioException)) return false;
            var errorCode = Marshal.GetHRForException(ioException) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33; // 32: Sharing violation, 33: Lock violation
        }
    }
}