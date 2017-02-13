using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyPS
{
    static class Utils
    {
        public static bool RunCommand(string commandName, string parameterString, out string errorReason)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = String.Format("-ExecutionPolicy Bypass -Command \"& \\\"{0}\\\" {1}; {2}; {3};\"",
                commandName,
                parameterString.Replace("\"", "\\\""),
                "Write-Host \\\"Press any key to continue ...\\\"",
                "$host.UI.RawUI.ReadKey(\\\"NoEcho, IncludeKeyDown\\\") | Out-Null");
            start.UseShellExecute = true;
            start.FileName = "powershell.exe";

            try
            {
                using (Process proc = Process.Start(start))
                {
                    proc.WaitForExit();
                }
                errorReason = "";
                return true;
            }
            catch (FileNotFoundException)
            {
                errorReason = "Could not find powershell.exe";
                return false;
            }
            catch (Exception ex)
            {
                errorReason = "An exception occured when running Powershell:\n" + ex.Message;
                return false;
            }
        }
    }
}
