using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using System.Windows.Forms;

namespace EasyPS
{
    public class PSCommandContainer : IDisposable
    {
        // Common parameters.
        //Verbose System.Management.Automation.SwitchParameter
        //Debug System.Management.Automation.SwitchParameter
        //ErrorAction System.Management.Automation.ActionPreference
        //WarningAction System.Management.Automation.ActionPreference
        //InformationAction System.Management.Automation.ActionPreference
        //ErrorVariable System.String
        //WarningVariable System.String
        //InformationVariable System.String
        //OutVariable System.String
        //OutBuffer System.Int32
        //PipelineVariable System.String
        protected static readonly HashSet<string> CommonParametersSet = new HashSet<string> {
            "Verbose",
            "Debug",
            "ErrorAction",
            "WarningAction",
            "InformationAction",
            "ErrorVariable",
            "WarningVariable",
            "InformationVariable",
            "OutVariable",
            "OutBuffer",
            "PipelineVariable" };
        protected Process HelpProcess { get; set; }
        protected List<RowContainer> ParameterRowInfos = new List<RowContainer>();

        protected IHistory History { get; set; }

        public IEnumerable<ParameterMetadata> Parameters { get; protected set; }
        public IEnumerable<ParameterMetadata> CommonParameters { get; protected set; }
        public string CommandName { get; protected set; }

        public PSCommandContainer(string commandName)
        {
            this.CommandName = commandName;
        }

        public bool Setup(IHistory history)
        {
            this.History = history;

            try
            {
                using (PowerShell psInstance = PowerShell.Create())
                {
                    Command executionCmd = new Command("Set-ExecutionPolicy");
                    executionCmd.Parameters.Add("ExecutionPolicy", "Unrestricted");
                    executionCmd.Parameters.Add("Scope", "Process");

                    psInstance.Commands.AddCommand(executionCmd);
                    psInstance.Invoke();

                    psInstance.AddScript($"Get-Command -Name \"{CommandName}\"");
                    Collection<PSObject> getCommandOutput = psInstance.Invoke();

                    if (getCommandOutput.Count >= 1 && getCommandOutput[0].ImmediateBaseObject is CommandInfo)
                    {
                        CommandInfo scriptInfo = (CommandInfo)getCommandOutput[0].ImmediateBaseObject;

                        this.CommonParameters = from param in scriptInfo.Parameters
                                                where CommonParametersSet.Contains(param.Value.Name)
                                                select param.Value;
                        this.Parameters = from param in scriptInfo.Parameters
                                          where !CommonParametersSet.Contains(param.Value.Name)
                                          select param.Value;

                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to fetch command information for {CommandName}.");
                    }
                }
            }
            catch (ParseException)
            {
                MessageBox.Show("The script is malformed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An exception occured while reading the script:\n" + ex.Message);
            }

            return false;
        }

        public void AttachToDataGridView(DataGridView dataGridView, string nameColumn, string valueColumn)
        {
            foreach (ParameterMetadata parameter in Parameters)
            {
                int rowIndex = dataGridView.Rows.Add();
                var row = dataGridView.Rows[rowIndex];

                var nameCell = row.Cells[nameColumn];
                var valueCell = row.Cells[valueColumn];
                
                ParameterRowInfos.Add(new RowContainer(parameter, nameCell, valueCell, History, CommandName));
            }
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

        public bool RunCommand()
        {
            StringBuilder parameterString = new StringBuilder();
            foreach (RowContainer parameter in ParameterRowInfos)
            {
                if (parameter.IsSet)
                {
                    parameterString.Append(parameter.EscapedParameterString + " ");

                    if (parameter.CanUseHistory)
                    {
                        string value = parameter.ParameterValue;
                        if (History != null && value != null)
                        {
                            History.AddHistory(CommandName, parameter.ParameterMetadata.Name, value);
                        }
                    }
                }
            }

            ProcessStartInfo start = new ProcessStartInfo();
            start.Arguments = String.Format("-ExecutionPolicy Bypass -Command \"& \\\"{0}\\\" {1}; {2}; {3};\"",
                CommandName,
                parameterString.ToString().Replace("\"", "\\\""),
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
                return true;
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("Could not find powershell.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An exception occured when running Powershell:\n" + ex.Message);
            }
            return false;
        }

        public void Dispose()
        {
            CleanupHelpProcess(false);
        }
    }
}
