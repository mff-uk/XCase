namespace ConfigWizardUI
{
    partial class ExternalWizardPage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExternalWizardPage));
            this.wizardSidebar1 = new ConfigWizardUI.WizardSidebar();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // wizardSidebar1
            // 
            this.wizardSidebar1.Dock = System.Windows.Forms.DockStyle.Left;
            this.wizardSidebar1.Location = new System.Drawing.Point(0, 0);
            this.wizardSidebar1.Name = "wizardSidebar1";
            this.wizardSidebar1.Size = new System.Drawing.Size(185, 297);
            this.wizardSidebar1.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(165, 414);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // ExternalWizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.wizardSidebar1);
            this.Name = "ExternalWizardPage";
            this.Size = new System.Drawing.Size(571, 297);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private WizardSidebar wizardSidebar1;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
