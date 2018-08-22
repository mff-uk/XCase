namespace XCase.Translation.XmlSchema
{
    partial class StartTranslation
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
            this.browseButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.configFileBox = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.createConfigRButton = new System.Windows.Forms.RadioButton();
            this.useConfigRButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.newConfigButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // browseButton
            // 
            this.browseButton.Enabled = false;
            this.browseButton.Location = new System.Drawing.Point(414, 67);
            this.browseButton.Margin = new System.Windows.Forms.Padding(1, 1, 3, 3);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(64, 24);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(348, 2);
            this.OKButton.Margin = new System.Windows.Forms.Padding(3, 2, 2, 2);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(63, 24);
            this.OKButton.TabIndex = 3;
            this.OKButton.Text = "Run";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(414, 2);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(1, 2, 3, 2);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(64, 24);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // configFileBox
            // 
            this.configFileBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configFileBox.Enabled = false;
            this.configFileBox.Location = new System.Drawing.Point(23, 69);
            this.configFileBox.Margin = new System.Windows.Forms.Padding(23, 3, 3, 3);
            this.configFileBox.Name = "configFileBox";
            this.configFileBox.ReadOnly = true;
            this.configFileBox.Size = new System.Drawing.Size(387, 20);
            this.configFileBox.TabIndex = 6;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tableLayoutPanel1.Controls.Add(this.configFileBox, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.browseButton, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.createConfigRButton, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.useConfigRButton, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(13, 17);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(481, 145);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // createConfigRButton
            // 
            this.createConfigRButton.AutoSize = true;
            this.createConfigRButton.Location = new System.Drawing.Point(5, 23);
            this.createConfigRButton.Margin = new System.Windows.Forms.Padding(5, 1, 3, 1);
            this.createConfigRButton.Name = "createConfigRButton";
            this.createConfigRButton.Size = new System.Drawing.Size(141, 17);
            this.createConfigRButton.TabIndex = 7;
            this.createConfigRButton.TabStop = true;
            this.createConfigRButton.Text = "use default configuration";
            this.createConfigRButton.UseVisualStyleBackColor = true;
            this.createConfigRButton.CheckedChanged += new System.EventHandler(this.createConfigRButton_CheckedChanged);
            // 
            // useConfigRButton
            // 
            this.useConfigRButton.AutoSize = true;
            this.useConfigRButton.Location = new System.Drawing.Point(5, 45);
            this.useConfigRButton.Margin = new System.Windows.Forms.Padding(5, 1, 3, 1);
            this.useConfigRButton.Name = "useConfigRButton";
            this.useConfigRButton.Size = new System.Drawing.Size(129, 17);
            this.useConfigRButton.TabIndex = 8;
            this.useConfigRButton.TabStop = true;
            this.useConfigRButton.Text = "use configuration from";
            this.useConfigRButton.UseVisualStyleBackColor = true;
            this.useConfigRButton.CheckedChanged += new System.EventHandler(this.useConfigRButton_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0, 0, 3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(351, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Click \'Run\' to translate the PSM diagram into the XML Schema language.";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.tableLayoutPanel2.Controls.Add(this.cancelButton, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.OKButton, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.newConfigButton, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(13, 134);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(481, 28);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // newConfigButton
            // 
            this.newConfigButton.Location = new System.Drawing.Point(3, 2);
            this.newConfigButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.newConfigButton.Name = "newConfigButton";
            this.newConfigButton.Size = new System.Drawing.Size(114, 24);
            this.newConfigButton.TabIndex = 5;
            this.newConfigButton.Text = "New configuration";
            this.newConfigButton.UseVisualStyleBackColor = true;
            this.newConfigButton.Click += new System.EventHandler(this.newConfigButton_Click);
            // 
            // StartTranslation
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(504, 170);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(16, 181);
            this.Name = "StartTranslation";
            this.Padding = new System.Windows.Forms.Padding(13, 17, 10, 8);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "XML Schema Translation";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox configFileBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.RadioButton createConfigRButton;
        private System.Windows.Forms.RadioButton useConfigRButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button newConfigButton;
    }
}