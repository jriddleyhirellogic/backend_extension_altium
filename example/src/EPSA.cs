using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using DXP;
using EDP;
using PCB;
using SCH;

using ComponentData;

namespace EPSA
{
    public class EPSAOpener
    {
        // CancellationTokenSource to cancel the keep-alive loop.
        private readonly CancellationTokenSource _connectionCancellationTokenSource = new CancellationTokenSource();
        // Create the object
        ComponentDataExtractor extractor = new ComponentDataExtractor(compSelection: false);

        /// <summary>
        /// This is the public command method that Altium will call.
        /// </summary>
        public async Task ExecuteAsync()
        {
            Log("EPSA command started. (Electron integration disabled during development)");

            // Simulate a running process with a keep-alive loop.
            Task connectionTask = KeepConnectionAliveAsync(_connectionCancellationTokenSource.Token);

            // For development, simulate running for 10 seconds.
            Log("Simulating integration for 10 seconds...");
            await Task.Delay(10000).ConfigureAwait(false);
            Log("After Task.Delay...");

            // Cancel the keep-alive loop after the simulation period.
            _connectionCancellationTokenSource.Cancel();
            try
            {
                await connectionTask;
            }
            catch (TaskCanceledException)
            {
                Log("Keep-alive loop cancelled.");
            }

            Log("EPSA command shutdown complete.");
        }

        /// <summary>
        /// Logs messages to Altium's output (using the DXP API).
        /// </summary>
        public static void Log(string text)
        {
            // Ensure that DXP.GlobalVars.DXPWorkSpace is initialized.
            DXP.GlobalVars.DXPWorkSpace.DM_AddOutputLine(text, false, false);
        }

        /// <summary>
        /// A background loop representing ongoing connection or keep-alive work.
        /// </summary>
        private async Task KeepConnectionAliveAsync(CancellationToken token)
        {
            Log("Connection keep-alive loop started.");
            
            while (!token.IsCancellationRequested)
            {
                Log("Connection is active...");

                // Run the extraction in a separate background thread
                await Task.Run(() => extractor.Run());

                await Task.Delay(5000, token); // Allows cancellation to work
            }

            Log("Connection keep-alive loop stopped.");
        }

    }
}
