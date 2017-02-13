using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EasyPS
{
    // TODO: 
    // Split common parameters into second tab. Or have a combo box for showing them.
    // Checkbox for ("Add readchar...).
    // Fix warning icon.
    // Add pre-validation of numeric typed parameters.

    //class DataGridViewTextBoxCellEx : DataGridViewTextBoxCell
    //{
    //    protected override void PaintErrorIcon(Graphics graphics, Rectangle clipBounds, Rectangle cellValueBounds, string errorText)
    //    {
    //        if (String.IsNullOrEmpty(errorText))
    //        {
    //            graphics.FillRectangle(Brushes.Yellow, new Rectangle(cellValueBounds.Width - 10, 0, 10, 10));
    //        }
    //        //base.PaintErrorIcon(graphics, clipBounds, cellValueBounds, errorText);
    //    }
    //}

    public partial class EasyPSForm : Form
    {
        protected CommandController Controller { get; set; }
        protected HelpViewer HelpViewer { get; set; }
        protected HashSet<int> FreeFormRows = new HashSet<int>();
        protected HashSet<int> CheckBoxRows = new HashSet<int>();

        const string NameColumnID = "nameColumn";
        const string ValueColumnID = "valueColumn";

        public EasyPSForm(CommandController controller, HelpViewer helpViewer)
        {
            InitializeComponent();

            this.Controller = controller;
            this.HelpViewer = helpViewer;

            PopulateDataGrid(dataGridParameters);
        }

        protected void PopulateDataGrid(DataGridView dataGridView)
        {
            var nameColumn = new DataGridViewColumn(new DataGridViewTextBoxCell());
            nameColumn.Name = NameColumnID;
            nameColumn.HeaderText = "Parameter Name";
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            nameColumn.Width = 157;
            nameColumn.MinimumWidth = 157;
            nameColumn.ReadOnly = true;
            dataGridView.Columns.Add(nameColumn);

            var valueColumn = new DataGridViewColumn(new DataGridViewTextBoxCell());
            valueColumn.Name = ValueColumnID;
            valueColumn.HeaderText = "Value";
            dataGridView.Columns.Add(valueColumn);
            
            foreach (ParameterController parameter in Controller.Parameters)
            {
                // Don't include the common parameters for the moment.
                if (!parameter.IsCommonParameter)
                {
                    int rowIndex = dataGridView.Rows.Add();
                    var row = dataGridView.Rows[rowIndex];

                    var nameCell = row.Cells[NameColumnID];
                    var valueCell = row.Cells[ValueColumnID];

                    nameCell.Value = parameter.Name;

                    if (parameter.IsMandatory)
                    {
                        nameCell.ErrorText = "This parameter is mandatory";
                    }

                    if (parameter is BooleanParameterController)
                    {
                        DataGridViewCheckBoxCell CheckBoxCell = new DataGridViewCheckBoxCell();
                        CheckBoxCell.ThreeState = true;
                        CheckBoxCell.TrueValue = true;
                        CheckBoxCell.FalseValue = false;
                        CheckBoxCell.IndeterminateValue = ""; // Do not seem to be able to set null here.
                        CheckBoxCell.Value = "";

                        dataGridView[valueCell.ColumnIndex, rowIndex] = CheckBoxCell;

                        CheckBoxRows.Add(rowIndex);
                    }
                    else if (parameter is TextParameterController)
                    {
                        var textParameter = (TextParameterController)parameter;

                        var items = textParameter.Values;
                        if (items.Count != 0)
                        {
                            MakeComboBoxCell(dataGridView, valueCell.ColumnIndex, rowIndex, items, textParameter.IsMandatory);
                            if (textParameter.AllowFreeFormText)
                            {
                                FreeFormRows.Add(rowIndex);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Unexpected ParameterController");
                    }

                    if (!String.IsNullOrEmpty(parameter.HelpText))
                    {
                        valueCell.ToolTipText = parameter.HelpText;
                    }

                    valueCell.ReadOnly = false;
                }
            }

            dataGridView.EditingControlShowing += HandleEditingControlShowing;

            ResizeComponents();
        }

        protected void ResizeComponents()
        {
            int height = 0;
            height += dataGridParameters.ColumnHeadersHeight;
            foreach (DataGridViewRow row in dataGridParameters.Rows)
            {
                height += row.Height;
            }
            dataGridParameters.Height = height;

            ClientSize = new System.Drawing.Size(Width, dataGridParameters.Top + dataGridParameters.Height + 40);
        }
        
        protected void MakeComboBoxCell(DataGridView dataGridView, int columnIndex, int rowIndex, IEnumerable<String> items,
                bool isMandatory)
        {
            DataGridViewComboBoxCell comboBoxCell = new DataGridViewComboBoxCell();

            if (!isMandatory)
            {
                comboBoxCell.Items.Add("");
            }

            foreach (var item in items)
            {
                comboBoxCell.Items.Add(item);
            }

            comboBoxCell.Value = null;
            
            // if allow free form:
            // comboBoxCell.FlatStyle = FlatStyle.Flat;

            dataGridView[columnIndex, rowIndex] = comboBoxCell;
        }
        
        /// <summary>
        /// This allows free formed text in DataGridView ComboBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var valueCell = dataGridParameters.CurrentCell;
            
            if (FreeFormRows.Contains(valueCell.RowIndex) &&
                valueCell.IsInEditMode && 
                e.Control.GetType() == typeof(DataGridViewComboBoxEditingControl))
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
            var valueCell = dataGridParameters.CurrentCell;
            if (valueCell.IsInEditMode && sender is DataGridViewComboBoxEditingControl)
            {
                DataGridViewComboBoxEditingControl editControl = (DataGridViewComboBoxEditingControl)sender;
                editControl.Items.Add(editControl.Text);
                // Add new item to the drop down list.
                string value = editControl.Text;
                if (!((DataGridViewComboBoxCell)valueCell).Items.Contains(value))
                {
                    ((DataGridViewComboBoxCell)valueCell).Items.Insert(0, value);
                }
                valueCell.Value = editControl.Text;
            }
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            Hide();

            Dictionary<string, object> parameterValues = new Dictionary<string, object>();

            for (int i = 0; i < dataGridParameters.Rows.Count; i++)
            {
                var nameCell = dataGridParameters.Rows[i].Cells[NameColumnID];
                var valueCell = dataGridParameters.Rows[i].Cells[ValueColumnID];

                if (valueCell.Value != null && 
                    (!CheckBoxRows.Contains(i) || !(valueCell.Value is string))) // Checkbox valueCell.Value is "" if empty.
                {
                    parameterValues.Add((string)nameCell.Value, valueCell.Value);
                }
            }

            string parameterString = Controller.BuildParameterStringAndSaveHistory(parameterValues);

            string errorReason;
            if (!Utils.RunCommand(Controller.CommandName, parameterString, out errorReason))
            {
                MessageBox.Show(errorReason);
            }

            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void EasyPSForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            HelpViewer.ShowHelpWindow();
            e.Cancel = true;
        }
    }
}
