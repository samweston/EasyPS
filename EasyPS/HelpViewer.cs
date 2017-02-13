using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace EasyPS
{
    public class HelpViewer : IDisposable
    {
        protected Process HelpProcess { get; set; }
        public string CommandName { get; protected set; }

        public HelpViewer(string commandName)
        {
            this.CommandName = commandName;
        }

        protected void CleanupHelpProcess(bool waitForExit)
        {
            if (HelpProcess != null)
            {
                if (!HelpProcess.HasExited)
                {
                    try { HelpProcess.Kill(); } catch { }
                }
                if (waitForExit)
                {
                    HelpProcess.WaitForExit();
                }
                HelpProcess.Dispose();
            }
        }

        public void ShowHelpWindow()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = String.Format("-ExecutionPolicy Bypass \"Get-Help -Name \\\"{0}\\\" -ShowWindow; {1};\"",
                CommandName,
                "$host.UI.RawUI.ReadKey(\\\"NoEcho, IncludeKeyDown\\\") | Out-Null");
            start.CreateNoWindow = true;
            start.UseShellExecute = true;
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.FileName = "powershell.exe";

            try
            {
                CleanupHelpProcess(true);
                HelpProcess = Process.Start(start);
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Could not find powershell.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An exception occured when running Powershell:\n" + ex.Message);
            }
        }


        public void Dispose()
        {
            CleanupHelpProcess(false);
        }
    }
}
