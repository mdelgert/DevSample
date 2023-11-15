using System;
using System.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace DevSample
{
    class Program
    {
        static readonly string _logFile;
        static readonly int _cyclesToRun;
        static readonly int _samplesToLoad;
        static readonly DateTime _sampleStartDate;
        static readonly TimeSpan _sampleIncrement;
        
        static Program()
        {
            //Note: these settings should not be modified
            _logFile = $"{ConfigurationManager.AppSettings["LogFilePath"]}\\{DateTime.Now:yyyyMMddHHmmss}_log.txt";
            _cyclesToRun = Environment.ProcessorCount  > 1 ? Environment.ProcessorCount / 2 : 1; //hopefully we have more than 1 core to work with, run cores/2 cycles with a max of 4 cycles
            _cyclesToRun = _cyclesToRun > 4 ? 4 : _cyclesToRun;
            _samplesToLoad = 222222;
            _sampleStartDate = new DateTime(1990, 1, 1, 1, 1, 1, 1);
            _sampleIncrement = new TimeSpan(0, 5, 0);
        }

        static void Main(string[] args)
        {
            Stopwatch totalMonitor = new Stopwatch();
            totalMonitor.Start();

            LogMessage($"Starting Execution on a {Environment.ProcessorCount} core system. A total of {_cyclesToRun} cycles will be run");

            //400% performance increase
            //for (int i = 0; i < _cyclesToRun; i++)
            Parallel.For(0, _cyclesToRun, i =>
            {
                try
                {

                    TimeSpan cycleElapsedTime = new TimeSpan();

                    Stopwatch cycleTimer = new Stopwatch();

                    SampleGenerator sampleGenerator = new SampleGenerator(_sampleStartDate, _sampleIncrement, _samplesToLoad);

                    LogMessage($"Cycle {i} Started Sample Load.");


                    cycleTimer.Start();

                    sampleGenerator.LoadSamples(_samplesToLoad);

                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;

                    LogMessage($"Cycle {i} Finished Sample Load. Load Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");


                    LogMessage($"Cycle {i} Started Sample Validation.");


                    cycleTimer.Restart();

                    sampleGenerator.ValidateSamples();

                    cycleTimer.Stop();
                    cycleElapsedTime = cycleTimer.Elapsed;

                    LogMessage($"Cycle {i} Finished Sample Validation. Total Samples Validated: {sampleGenerator.SamplesValidated}. Validation Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");



                    //float valueSum = 0;
                    decimal valueSum = 0;

                    foreach (Sample s in sampleGenerator.Samples)
                    {
                        valueSum += s.Value;
                    }

                    //Complete: why do we only seem to get 7 digits of precision? The CEO wants to see at least 20!
                    //LogMessage($"Cycle {i} Sum of All Samples: {valueSum.ToString("N")}.");
                    LogMessage($"Cycle {i} Sum of All Samples: {valueSum:N}.");

                    LogMessage($"Cycle {i} Finished. Total Cycle Time: {cycleElapsedTime.TotalMilliseconds.ToString("N")} ms.");

                }
                catch (Exception ex)
                {
                    LogMessage($"Execution Failed!\n{ex.ToString()}");
                }

            });

            totalMonitor.Stop();

            LogMessage("-----");
            LogMessage($"Execution Finished. Total Elapsed Time: {totalMonitor.Elapsed.TotalMilliseconds.ToString("N")} ms.");


            Console.Read();

        }

        static void LogMessage(string message)
        {
            var logMessage = $"{DateTime.Now:HH:mm:ss.fffff} - {message}";
            Console.WriteLine(logMessage);
            LogToFile(logMessage);
        }

        static void LogToFile(string message)
        {
            //Complete: implement this when someone complains about it not working... 
            //everything written to the console should also be written to a log under
            //C:\Temp. A new log with a unique file name should be created each time the application is run.

            const int maxRetries = 10;
            int retries = 0;

            while (retries < maxRetries)
            {
                try
                {
                    // Append the log entry to the log file
                    File.AppendAllText(_logFile, message + Environment.NewLine);
                    return; // If successful, exit the loop
                }
                catch (IOException ex) when (IsFileLocked(ex))
                {
                    // File is locked, wait a moment and then retry
                    Thread.Sleep(10);
                    retries++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error appending to log file: {ex.Message}");
                    return; // Exit the loop on any other exception
                }
            }

            Console.WriteLine($"Failed to append to log file after {maxRetries} retries.");
        }

        // Helper method to check if the exception is due to a locked file
        static bool IsFileLocked(Exception ex)
        {
            if (ex is IOException ioException)
            {
                int errorCode = Marshal.GetHRForException(ioException) & ((1 << 16) - 1);
                return errorCode == 32 || errorCode == 33; // 32: Sharing violation, 33: Lock violation
            }

            return false;
        }
    }
}