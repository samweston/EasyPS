using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace EasyPS
{
    public abstract class ParameterController
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

        public string CommandName { get; protected set; }
        protected IHistory History { get; set; }

        protected ParameterMetadata Metadata { get; set; }
        protected ParameterAttribute Attribute { get; set; }
        protected ValidateSetAttribute _ValidateSetAttribute { get; set; }

        protected ParameterController(ParameterMetadata metadata, string commandName, IHistory history)
        {
            this.Metadata = metadata;
            this.Attribute = GetMetadataAttributeOfType<ParameterAttribute>();
            this._ValidateSetAttribute = GetMetadataAttributeOfType<ValidateSetAttribute>();
            this.CommandName = commandName;
            this.History = history;
        }

        public static ParameterController Create(ParameterMetadata metadata, string commandName, IHistory history)
        {
            ParameterController paramController;
            
            if (metadata.ParameterType == typeof(bool) || metadata.SwitchParameter)
            {
                paramController = new BooleanParameterController(metadata, commandName, history);
            }
            else
            {
                paramController = new TextParameterController(metadata, commandName, history);
            }
            
            return paramController;
        }

        /// <summary>
        /// Escape parameter text so that it can be run in a powershell command.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string EscapeParameterString(object value)
        {
            if (Metadata.SwitchParameter)
            {
                if (value is bool)
                {
                    return "-" + Metadata.Name;
                }
                else
                {
                    throw new Exception("Unexpected type.");
                }
            }
            else
            {
                if (value is string)
                {
                    return $"-{Metadata.Name} \"{((string)value).Replace("\"", "`\"")}\"";
                }
                else if (value is bool)
                {
                    string stringValue = (bool)value ? "$true" : "$false";
                    return $"-{Metadata.Name} {stringValue}";
                }
                else
                {
                    throw new Exception("Unexpected type.");
                }
            }
        }

        protected T GetMetadataAttributeOfType<T>() where T : Attribute
        {
            return (T)Metadata.Attributes.SingleOrDefault(a => a is T);
        }

        public bool IsCommonParameter
        {
            get
            {
                return CommonParametersSet.Contains(Name);
            }
        }
        
        public bool IsMandatory
        {
            get
            {
                return Attribute.Mandatory;
            }
        }

        public string HelpText
        {
            get
            {
                return Attribute.HelpMessage;
            }
        }

        public string Name
        {
            get
            {
                return Metadata.Name;
            }
        }

        public virtual bool CanUseHistory
        {
            get
            {
                return false;
            }
        }
    }

    internal class BooleanParameterController : ParameterController
    {
        public BooleanParameterController(ParameterMetadata metadata, string commandName, IHistory history) : base(metadata, commandName, history)
        {
        }
    }

    internal class TextParameterController : ParameterController
    {
        public TextParameterController(ParameterMetadata metadata, string commandName, IHistory history) : base(metadata, commandName, history)
        {
        }

        public override bool CanUseHistory
        {
            get
            {
                return !Metadata.ParameterType.IsEnum;
            }
        }

        public bool AllowFreeFormText
        {
            get
            {
                return !Metadata.ParameterType.IsEnum && _ValidateSetAttribute == null;
            }
        }
        
        /// <summary>
        /// Get available values for this parameters.
        /// </summary>
        public IList<string> Values
        {
            get
            {
                if (Metadata.ParameterType.IsEnum)
                {
                    return Metadata.ParameterType.GetEnumNames().ToList();
                }
                else if (_ValidateSetAttribute != null)
                {
                    // Has a validate set, so build options using that.
                    return _ValidateSetAttribute.ValidValues;
                }
                else
                {
                    // Regular text field. Use history to build a list.
                    return History.GetHistory(CommandName, Metadata.Name);
                }
            }
        }
    }
}
