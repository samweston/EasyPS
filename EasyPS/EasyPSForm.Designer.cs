namespace EasyPS
{
    partial class EasyPSForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.runButton = new System.Windows.Forms.Button();
            this.dataGridParameters = new System.Windows.Forms.DataGridView();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridParameters)).BeginInit();
            this.SuspendLayout();
            // 
            // runButton
            // 
            this.runButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.runButton.Location = new System.Drawing.Point(199, 210);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(72, 23);
            this.runButton.TabIndex = 1;
            this.runButton.Text = "&Run";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // dataGridParameters
            // 
            this.dataGridParameters.AllowUserToAddRows = false;
            this.dataGridParameters.AllowUserToDeleteRows = false;
            this.dataGridParameters.AllowUserToResizeRows = false;
            this.dataGridParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridParameters.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridParameters.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridParameters.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridParameters.Location = new System.Drawing.Point(12, 12);
            this.dataGridParameters.Name = "dataGridParameters";
            this.dataGridParameters.RowHeadersVisible = false;
            this.dataGridParameters.Size = new System.Drawing.Size(337, 55);
            this.dataGridParameters.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(277, 210);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(72, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "&Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // EasyPSForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(361, 245);
            this.Controls.Add(this.dataGridParameters);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.cancelButton);
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EasyPSForm";
            this.ShowIcon = false;
            this.Text = "Enter Parameters";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.EasyPSForm_HelpButtonClicked);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridParameters)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.DataGridView dataGridParameters;
        private System.Windows.Forms.Button cancelButton;
    }
}

