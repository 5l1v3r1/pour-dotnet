using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace Pour.Client.Library.Test.Integration
{
    /// <summary>
    /// This helps starting storage emulator and stopping it during the tests
    /// </summary>
    internal static class AzureStorageEmulatorManager
    {
        private static string Folder = ConfigurationManager.AppSettings["EmulatorDirectory"];

        /// <summary>
        /// Starts the emulator
        /// </summary>
        public static void Start()
        {
            Execute("start");
            Execute("clear all");
        }

        /// <summary>
        /// Stops the emulator
        /// </summary>
        public static void Stop()
        {
            Execute("stop");
        }

        #region Private helpers

        private static void Execute(string command)
        {
            using (Process process = Process.Start(Create(command)))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Unable to start process.");
                }

                process.WaitForExit();
            }
        }

        private static ProcessStartInfo Create(string command)
        {
            // Azure SDK 2.5 Storage Emulator file name is WAStorageEmulator, for upper versions it is AzureStorageEmulator
            string filepathToEmulator = Path.Combine(Folder, "AzureStorageEmulator.exe");
            string filename = File.Exists(filepathToEmulator) ? filepathToEmulator : Path.Combine(Folder, "WAStorageEmulator.exe");

            return new ProcessStartInfo
            {
                FileName = filename,
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }

        #endregion
    }
}