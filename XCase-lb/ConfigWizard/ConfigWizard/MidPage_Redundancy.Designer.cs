namespace ConfigWizard
{
    partial class MidPage_Redundancy
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
            this.elimRedAPsChkBox = new System.Windows.Forms.CheckBox();
            this.elimRedAttrDeclsChkBox = new System.Windows.Forms.CheckBox();
            this.elimRedInNestingsChkBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.Size = new System.Drawing.Size(457, 59);
            this.wizardBanner1.Subtitle = "Define which algorithms for redundancy elimination you want to be applied.";
            this.wizardBanner1.Title = "Redundancy in XML Schema";
            // 
            // elimRedAPsChkBox
            // 
            this.elimRedAPsChkBox.AutoSize = true;
            this.elimRedAPsChkBox.Location = new System.Drawing.Point(38, 97);
            this.elimRedAPsChkBox.Name = "elimRedAPsChkBox";
            this.elimRedAPsChkBox.Size = new System.Drawing.Size(261, 17);
            this.elimRedAPsChkBox.TabIndex = 3;
            this.elimRedAPsChkBox.Text = "Run the algorithm for elimination of redundant APs";
            this.elimRedAPsChkBox.UseVisualStyleBackColor = true;
            this.elimRedAPsChkBox.CheckedChanged += new System.EventHandler(this.elimRedAPsChkBox_CheckedChanged);
            // 
            // elimRedAttrDeclsChkBox
            // 
            this.elimRedAttrDeclsChkBox.AutoSize = true;
            this.elimRedAttrDeclsChkBox.Location = new System.Drawing.Point(38, 120);
            this.elimRedAttrDeclsChkBox.Name = "elimRedAttrDeclsChkBox";
            this.elimRedAttrDeclsChkBox.Size = new System.Drawing.Size(340, 17);
            this.elimRedAttrDeclsChkBox.TabIndex = 4;
            this.elimRedAttrDeclsChkBox.Text = "Run the algorithm for elimination of redundant attribute declarations";
            this.elimRedAttrDeclsChkBox.UseVisualStyleBackColor = true;
            this.elimRedAttrDeclsChkBox.CheckedChanged += new System.EventHandler(this.elimRedAttrDeclsChkBox_CheckedChanged);
            // 
            // elimRedInNestingsChkBox
            // 
            this.elimRedInNestingsChkBox.AutoSize = true;
            this.elimRedInNestingsChkBox.Enabled = false;
            this.elimRedInNestingsChkBox.Location = new System.Drawing.Point(38, 143);
            this.elimRedInNestingsChkBox.Name = "elimRedInNestingsChkBox";
            this.elimRedInNestingsChkBox.Size = new System.Drawing.Size(355, 17);
            this.elimRedInNestingsChkBox.TabIndex = 5;
            this.elimRedInNestingsChkBox.Text = "Run the algorithm for elimination of redundancy in nestings of choices.";
            this.elimRedInNestingsChkBox.UseVisualStyleBackColor = true;
            this.elimRedInNestingsChkBox.CheckedChanged += new System.EventHandler(this.elimRedInNestingsChkBox_CheckedChanged);
            // 
            // MidPage_Redundancy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.elimRedInNestingsChkBox);
            this.Controls.Add(this.elimRedAttrDeclsChkBox);
            this.Controls.Add(this.elimRedAPsChkBox);
            this.Name = "MidPage_Redundancy";
            this.Size = new System.Drawing.Size(457, 253);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.elimRedAPsChkBox, 0);
            this.Controls.SetChildIndex(this.elimRedAttrDeclsChkBox, 0);
            this.Controls.SetChildIndex(this.elimRedInNestingsChkBox, 0);
            this.Controls.SetChildIndex(this.wizardBanner1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox elimRedAPsChkBox;
        private System.Windows.Forms.CheckBox elimRedAttrDeclsChkBox;
        private System.Windows.Forms.CheckBox elimRedInNestingsChkBox;




    }
}
