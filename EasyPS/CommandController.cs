using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace EasyPS
{
    public class CommandController
    {
        protected List<ParameterController> InternalParameters = new List<ParameterController>();

        public string CommandName { get; protected set; }
        public IHistory History { get; protected set; }

        public CommandController(string commandName, IHistory history)
        {
            this.CommandName = commandName;
            this.History = history;
        }

        public ReadOnlyCollection<ParameterController> Parameters
        {
            get
            {
                return InternalParameters.AsReadOnly();
            }
        }

        public bool PopulateParameters(out string reason)
        {
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
                        
                        foreach (KeyValuePair<string, ParameterMetadata> kvp in scriptInfo.Parameters)
                        {
                            InternalParameters.Add(ParameterController.Create(kvp.Value, CommandName, History));
                        }

                        reason = "";
                        return true;
                    }
                    else
                    {
                        reason = $"Failed to fetch command information for {CommandName}.";
                    }
                }
            }
            catch (ParseException)
            {
                reason = "The script is malformed.";
            }
            catch (Exception ex)
            {
                reason = "An exception occured while reading the script:\n" + ex.Message;
            }

            return false;
        }

        public string BuildParameterStringAndSaveHistory(Dictionary<string, object> parameterValues)
        {
            StringBuilder parameterString = new StringBuilder();

            foreach (var parameterValue in parameterValues)
            {
                var parameterController = Parameters.First(pc => pc.Name == parameterValue.Key);

                // Use controller to get the escaped string.
                parameterString.Append(parameterController.EscapeParameterString(parameterValue.Value) + " ");

                // Use controller to check if can save history.
                if (parameterController.CanUseHistory && History != null && parameterValue.Value != null &&
                    parameterValue.Value is string)
                {
                    History.AddHistory(CommandName, parameterValue.Key, (string)parameterValue.Value);
                }
            }

            return parameterString.ToString();
        }
    }
}
