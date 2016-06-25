using System;
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
        protected string CommandName { get; set; }
        protected PSCommandContainer CommandInfo { get; set; }

        public EasyPSForm(string commandName, PSCommandContainer commandParameters)
        {
            InitializeComponent();

            this.CommandName = commandName;
            this.CommandInfo = commandParameters;

            PopulateDataGrid(dataGridParameters);
        }

        protected void PopulateDataGrid(DataGridView dataGridView)
        {
            const string nameColumnID = "nameColumn";
            const string valueColumnID = "valueColumn";

            var nameColumn = new DataGridViewColumn(new DataGridViewTextBoxCell());
            nameColumn.Name = nameColumnID;
            nameColumn.HeaderText = "Parameter Name";
            nameColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            nameColumn.Width = 157;
            nameColumn.MinimumWidth = 157;
            nameColumn.ReadOnly = true;
            dataGridView.Columns.Add(nameColumn);

            var valueColumn = new DataGridViewColumn(new DataGridViewTextBoxCell());
            valueColumn.Name = valueColumnID;
            valueColumn.HeaderText = "Value";
            dataGridView.Columns.Add(valueColumn);

            CommandInfo.AttachToDataGridView(dataGridParameters, nameColumnID, valueColumnID);

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

        private void runButton_Click(object sender, EventArgs e)
        {
            Hide();
            CommandInfo.RunCommand();
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void EasyPSForm_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CommandInfo.ShowHelpWindow();
            e.Cancel = true;
        }
    }
}
