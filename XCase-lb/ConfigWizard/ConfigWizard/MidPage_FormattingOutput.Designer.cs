namespace ConfigWizard
{
    partial class MidPage_FormattingOutput
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
            this.emptyLineChkBox = new System.Windows.Forms.CheckBox();
            this.cntIndentSpacesBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.Size = new System.Drawing.Size(490, 59);
            this.wizardBanner1.Subtitle = "Please check options that you want to be applied.";
            this.wizardBanner1.Title = "Output formatting";
            // 
            // emptyLineChkBox
            // 
            this.emptyLineChkBox.AutoSize = true;
            this.emptyLineChkBox.Checked = true;
            this.emptyLineChkBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.emptyLineChkBox.Location = new System.Drawing.Point(35, 84);
            this.emptyLineChkBox.Name = "emptyLineChkBox";
            this.emptyLineChkBox.Size = new System.Drawing.Size(252, 17);
            this.emptyLineChkBox.TabIndex = 2;
            this.emptyLineChkBox.Text = "put an empty line before each global declaration\r\n";
            this.emptyLineChkBox.UseVisualStyleBackColor = true;
            this.emptyLineChkBox.CheckedChanged += new System.EventHandler(this.emptyLineChkBox_CheckedChanged);
            // 
            // cntIndentSpacesBox
            // 
            this.cntIndentSpacesBox.Location = new System.Drawing.Point(344, 108);
            this.cntIndentSpacesBox.Name = "cntIndentSpacesBox";
            this.cntIndentSpacesBox.Size = new System.Drawing.Size(27, 20);
            this.cntIndentSpacesBox.TabIndex = 3;
            this.cntIndentSpacesBox.Text = "4";
            this.cntIndentSpacesBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.cntIndentSpacesBox.TextChanged += new System.EventHandler(this.cntIndentSpacesBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(32, 111);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(289, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Number of spaces for children indentation in XML document";
            // 
            // MidPage_FormattingOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cntIndentSpacesBox);
            this.Controls.Add(this.emptyLineChkBox);
            this.Name = "MidPage_FormattingOutput";
            this.Size = new System.Drawing.Size(490, 344);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.wizardBanner1, 0);
            this.Controls.SetChildIndex(this.emptyLineChkBox, 0);
            this.Controls.SetChildIndex(this.cntIndentSpacesBox, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox emptyLineChkBox;
        private System.Windows.Forms.TextBox cntIndentSpacesBox;
        private System.Windows.Forms.Label label1;

    }
}
