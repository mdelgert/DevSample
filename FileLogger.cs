using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DevSample
{
    internal class FileLogger
    {
        private static readonly string LogFile;

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

        private static bool IsFileLocked(Exception ex)
        {
            if (!(ex is IOException ioException)) return false;
            var errorCode = Marshal.GetHRForException(ioException) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33; // 32: Sharing violation, 33: Lock violation
        }
    }
}