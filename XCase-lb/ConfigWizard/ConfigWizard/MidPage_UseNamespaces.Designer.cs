namespace ConfigWizard
{
    partial class MidPage_UseNamespaces
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MidPage_UseNamespaces));
            this.yesRadioButton = new System.Windows.Forms.RadioButton();
            this.noRadioButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.Size = new System.Drawing.Size(490, 59);
            this.wizardBanner1.Subtitle = "Shall PIM packages be reflected in result using XML Namespaces?";
            this.wizardBanner1.Title = "Considering packages";
            // 
            // yesRadioButton
            // 
            this.yesRadioButton.AutoSize = true;
            this.yesRadioButton.Checked = true;
            this.yesRadioButton.Location = new System.Drawing.Point(36, 220);
            this.yesRadioButton.Name = "yesRadioButton";
            this.yesRadioButton.Size = new System.Drawing.Size(341, 17);
            this.yesRadioButton.TabIndex = 3;
            this.yesRadioButton.TabStop = true;
            this.yesRadioButton.Text = "Yes, use concept of XML Namespaces to represent PIM packages";
            this.yesRadioButton.UseVisualStyleBackColor = true;
            this.yesRadioButton.CheckedChanged += new System.EventHandler(this.yesRadioButton_CheckedChanged);
            // 
            // noRadioButton
            // 
            this.noRadioButton.AutoSize = true;
            this.noRadioButton.Location = new System.Drawing.Point(36, 246);
            this.noRadioButton.Name = "noRadioButton";
            this.noRadioButton.Size = new System.Drawing.Size(165, 17);
            this.noRadioButton.TabIndex = 4;
            this.noRadioButton.Text = "No, just ignore PIM packages";
            this.noRadioButton.UseVisualStyleBackColor = true;
            this.noRadioButton.CheckedChanged += new System.EventHandler(this.noRadioButton_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(417, 104);
            this.label1.TabIndex = 5;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // MidPage_UseNamespaces
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.noRadioButton);
            this.Controls.Add(this.yesRadioButton);
            this.Name = "MidPage_UseNamespaces";
            this.Size = new System.Drawing.Size(490, 282);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.wizardBanner1, 0);
            this.Controls.SetChildIndex(this.yesRadioButton, 0);
            this.Controls.SetChildIndex(this.noRadioButton, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton yesRadioButton;
        private System.Windows.Forms.RadioButton noRadioButton;
        private System.Windows.Forms.Label label1;

    }
}
