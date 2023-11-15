using System;
using System.Configuration;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DevSample
{
    /// <summary>
    /// File logger helper class.
    /// </summary>
    internal class FileLogger
    {
        public static string LogFile = $"{ConfigurationManager.AppSettings["LogFilePath"]}\\{DateTime.Now:yyyyMMddHHmmss}_log.txt";

        /// <summary>
        /// This method writes messages to the console and log file.
        /// </summary>
        /// <param name="message">Message param to append to console and log.</param>
        public static void LogMessage(string message)
        {
            var logMessage = $"{DateTime.Now:HH:mm:ss.fffff} - {message}";
            Console.WriteLine(logMessage);
            //Don't wait for the logger to write to file for more performance.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            LogToFileAsync(logMessage);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        /// <summary>
        /// Log to file async task.
        /// </summary>
        private static async Task LogToFileAsync(string message)
        {
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

        /// <summary>
        /// Helper method to check if the exception is due to a locked file.
        /// </summary>
        private static bool IsFileLocked(Exception ex)
        {
            if (!(ex is IOException ioException)) return false;
            var errorCode = Marshal.GetHRForException(ioException) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33; // 32: Sharing violation, 33: Lock violation
        }
    }
}