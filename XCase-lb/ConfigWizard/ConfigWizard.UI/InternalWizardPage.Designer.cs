namespace ConfigWizardUI
{
    partial class InternalWizardPage
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
            this.wizardBanner1 = new ConfigWizardUI.WizardBanner();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.BackColor = System.Drawing.SystemColors.Window;
            this.wizardBanner1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wizardBanner1.Dock = System.Windows.Forms.DockStyle.Top;
            this.wizardBanner1.Location = new System.Drawing.Point(0, 0);
            this.wizardBanner1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.wizardBanner1.Name = "wizardBanner1";
            this.wizardBanner1.Size = new System.Drawing.Size(438, 59);
            this.wizardBanner1.Subtitle = "Subtitle";
            this.wizardBanner1.TabIndex = 0;
            this.wizardBanner1.Title = "Title";
            // 
            // InternalWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.wizardBanner1);
            this.Name = "InternalWizardPage";
            this.Size = new System.Drawing.Size(438, 258);
            this.ResumeLayout(false);

        }

        #endregion

        public WizardBanner wizardBanner1;



    }
}
