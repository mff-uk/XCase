namespace ConfigWizard
{
    partial class MidPage_SaveConfiguration
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.configFileBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.dontSaveChkBox = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.Size = new System.Drawing.Size(490, 59);
            this.wizardBanner1.Subtitle = "Choose a file to which you want to save created configuration.";
            this.wizardBanner1.Title = "Save configuration";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Controls.Add(this.configFileBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.browseButton, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.dontSaveChkBox, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(35, 84);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(423, 55);
            this.tableLayoutPanel1.TabIndex = 9;
            // 
            // configFileBox
            // 
            this.configFileBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configFileBox.Location = new System.Drawing.Point(3, 3);
            this.configFileBox.Name = "configFileBox";
            this.configFileBox.Size = new System.Drawing.Size(357, 20);
            this.configFileBox.TabIndex = 6;
            // 
            // browseButton
            // 
            this.browseButton.Location = new System.Drawing.Point(366, 3);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(53, 24);
            this.browseButton.TabIndex = 1;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // dontSaveChkBox
            // 
            this.dontSaveChkBox.AutoSize = true;
            this.dontSaveChkBox.Location = new System.Drawing.Point(3, 33);
            this.dontSaveChkBox.Name = "dontSaveChkBox";
            this.dontSaveChkBox.Size = new System.Drawing.Size(118, 17);
            this.dontSaveChkBox.TabIndex = 8;
            this.dontSaveChkBox.Text = "don\'t save anything";
            this.dontSaveChkBox.UseVisualStyleBackColor = true;
            this.dontSaveChkBox.CheckedChanged += new System.EventHandler(this.dontSaveChkBox_CheckedChanged);
            // 
            // MidPage_SaveConfiguration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MidPage_SaveConfiguration";
            this.Size = new System.Drawing.Size(490, 335);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.wizardBanner1, 0);
            this.Controls.SetChildIndex(this.tableLayoutPanel1, 0);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox configFileBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.CheckBox dontSaveChkBox;


    }
}
