using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Windows.Forms;

namespace EasyPS
{
    public class RowContainer
    {
        protected IHistory History { get; set; }
        protected string CommandName { get; set; }

        protected DataGridViewCell NameCell { get; set; }

        protected DataGridViewCell ValueCell { get; set; }
        
        protected Lazy<ParameterAttribute> _parameterAttribute;
        protected ParameterAttribute ParameterAttribute
        {
            get
            {
                return _parameterAttribute.Value;
            }
        }

        protected Lazy<ValidateSetAttribute> _validateSetAttribute;
        protected ValidateSetAttribute ValidateSetAttribute
        {
            get
            {
                return _validateSetAttribute.Value;
            }
        }

        protected bool IsCheckBoxParameter()
        {
            return ParameterMetadata.ParameterType == typeof(bool) || ParameterMetadata.SwitchParameter;
        }

        public ParameterMetadata ParameterMetadata { get; set; }
        public bool CanUseHistory { get; protected set; }
        
        public String ParameterValue
        {
            get
            {
                if (ValueCell.Value is string)
                {
                    return (string)ValueCell.Value;
                }
                return null;
            }
        }

        public String EscapedParameterString
        {
            get
            {
                if (ParameterMetadata.SwitchParameter)
                {
                    if (ValueCell.Value is bool && (bool)ValueCell.Value)
                    {
                        return "-" + ParameterMetadata.Name;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (ValueCell.Value is string)
                    {
                        return $"-{ParameterMetadata.Name} \"{((string)ValueCell.Value).Replace("\"", "`\"")}\"";
                    }
                    else if (ValueCell.Value is bool)
                    {
                        string value = (bool)ValueCell.Value ? "$true" : "$false";
                        return $"-{ParameterMetadata.Name} {value}";
                    }
                    else
                    {
                        throw new Exception("Unexpected type.");
                    }
                }
            }
        }

        public bool IsSet
        {
            get
            {
                if (IsCheckBoxParameter())
                {
                    return !(ValueCell.Value is string); // Indeterminate ("" value).
                }
                else
                {
                    return ValueCell.Value != null;
                }
            }
        }

        protected T GetAttributeOfType<T>() where T : Attribute
        {
            return (T)ParameterMetadata.Attributes.SingleOrDefault(a => a is T);
        }

        protected void SetupNameCell(DataGridViewCell nameCell)
        {
            nameCell.Value = ParameterMetadata.Name;
            
            if (ParameterAttribute.Mandatory)
            {
                nameCell.ErrorText = "This parameter is mandatory";
            }

            this.NameCell = nameCell;
        }

        protected void MakeComboBoxCell(DataGridView dataGridView, int columnIndex, int rowIndex, IEnumerable<String> items, 
                bool allowFreeForm)
        {
            DataGridViewComboBoxCell comboBoxCell = new DataGridViewComboBoxCell();

            if (!ParameterAttribute.Mandatory)
            {
                comboBoxCell.Items.Add("");
            }

            foreach (var item in items)
            {
                comboBoxCell.Items.Add(item);
            }
            comboBoxCell.Value = null;
            

            if (allowFreeForm)
            {
                //comboBoxCell.FlatStyle = FlatStyle.Flat;
                dataGridView.EditingControlShowing += HandleEditingControlShowing;
            }

            dataGridView[columnIndex, rowIndex] = comboBoxCell;
        }

        /// <summary>
        /// This allows free formed text in DataGridView ComboBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (ValueCell.IsInEditMode && e.Control.GetType() == typeof(DataGridViewComboBoxEditingControl))
            {
                DataGridViewComboBoxEditingControl cbo =
                    e.Control as DataGridViewComboBoxEditingControl;
                cbo.DropDownStyle = ComboBoxStyle.DropDown;
                cbo.AutoCompleteMode = AutoCompleteMode.Suggest;
                cbo.Validating -= HandleValidating;
                cbo.Validating += HandleValidating;
            }
        }

        /// <summary>
        /// Runs on validation of input. Ensures that free formed text will be "saved" into 
        /// the control text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleValidating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ValueCell.IsInEditMode && sender is DataGridViewComboBoxEditingControl)
            {
                DataGridViewComboBoxEditingControl editControl = (DataGridViewComboBoxEditingControl)sender;
                editControl.Items.Add(editControl.Text);
                // Add new item to the drop down list.
                string value = editControl.Text;
                if (!((DataGridViewComboBoxCell)ValueCell).Items.Contains(value))
                {
                    ((DataGridViewComboBoxCell)ValueCell).Items.Insert(0, value);
                }
                ValueCell.Value = editControl.Text;
            }
        }

        protected void SetupValueCell(DataGridViewCell valueCell)
        {
            DataGridView dataGridView = valueCell.DataGridView;
            int columnIndex = valueCell.ColumnIndex;
            int rowIndex = valueCell.RowIndex;

            CanUseHistory = false;

            // Replace the existing value cell with an appropriate typed cell.
            if (IsCheckBoxParameter())
            {
                DataGridViewCheckBoxCell CheckBoxCell = new DataGridViewCheckBoxCell();
                CheckBoxCell.ThreeState = true;
                CheckBoxCell.TrueValue = true;
                CheckBoxCell.FalseValue = false;
                CheckBoxCell.IndeterminateValue = ""; // Do not seem to be able to set null here.
                CheckBoxCell.Value = "";

                dataGridView[columnIndex, rowIndex] = CheckBoxCell;
            }
            else if (ParameterMetadata.ParameterType.IsEnum)
            {
                MakeComboBoxCell(dataGridView, columnIndex, rowIndex,
                    ParameterMetadata.ParameterType.GetEnumNames().ToList(), false);
            }
            else if (ValidateSetAttribute != null)
            {
                MakeComboBoxCell(dataGridView, columnIndex, rowIndex,
                    ValidateSetAttribute.ValidValues, false);
            }
            else if (History != null)
            {
                CanUseHistory = true;

                // Use combo box with history.
                var historyItems = History.GetHistory(CommandName, ParameterMetadata.Name);
                if (historyItems != null)
                {
                    MakeComboBoxCell(dataGridView, columnIndex, rowIndex, historyItems, true);
                }
            }

            this.ValueCell = dataGridView[columnIndex, rowIndex];

            // This is the text that is brought up in the powershell prompt on "!?".
            if (!String.IsNullOrEmpty(ParameterAttribute.HelpMessage))
            {
                this.ValueCell.ToolTipText = ParameterAttribute.HelpMessage;
            }

            this.ValueCell.ReadOnly = false;
        }

        public RowContainer(ParameterMetadata parameterMetadata, DataGridViewCell nameCell, DataGridViewCell valueCell,
            IHistory history, string commandName)
        {
            this.History = history;
            this.CommandName = commandName;

            this.ParameterMetadata = parameterMetadata;
            this._parameterAttribute = new Lazy<ParameterAttribute>(() => GetAttributeOfType<ParameterAttribute>());
            this._validateSetAttribute = new Lazy<ValidateSetAttribute>(() => GetAttributeOfType<ValidateSetAttribute>());

            SetupNameCell(nameCell);
            SetupValueCell(valueCell);
        }
    }
}
