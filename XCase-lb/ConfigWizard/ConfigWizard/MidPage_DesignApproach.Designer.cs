namespace ConfigWizard
{
    partial class MidPage_DesignApproach
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
            this.venetianBlindRButton = new System.Windows.Forms.RadioButton();
            this.salamiSliceRButton = new System.Windows.Forms.RadioButton();
            this.gardenOfEdenRButton = new System.Windows.Forms.RadioButton();
            this.russianDollRButton = new System.Windows.Forms.RadioButton();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.Size = new System.Drawing.Size(490, 59);
            this.wizardBanner1.Subtitle = "Please select which pattern do you want to be applied.";
            this.wizardBanner1.Title = "Design pattern";
            // 
            // venetianBlindRButton
            // 
            this.venetianBlindRButton.AutoSize = true;
            this.venetianBlindRButton.Checked = true;
            this.venetianBlindRButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.venetianBlindRButton.Location = new System.Drawing.Point(35, 84);
            this.venetianBlindRButton.Name = "venetianBlindRButton";
            this.venetianBlindRButton.Size = new System.Drawing.Size(93, 17);
            this.venetianBlindRButton.TabIndex = 3;
            this.venetianBlindRButton.TabStop = true;
            this.venetianBlindRButton.Text = "Venetian Blind";
            this.venetianBlindRButton.UseVisualStyleBackColor = true;
            this.venetianBlindRButton.CheckedChanged += new System.EventHandler(this.venetianBlindRButton_CheckedChanged);
            // 
            // salamiSliceRButton
            // 
            this.salamiSliceRButton.AutoSize = true;
            this.salamiSliceRButton.Location = new System.Drawing.Point(35, 141);
            this.salamiSliceRButton.Name = "salamiSliceRButton";
            this.salamiSliceRButton.Size = new System.Drawing.Size(82, 17);
            this.salamiSliceRButton.TabIndex = 4;
            this.salamiSliceRButton.Text = "Salami Slice";
            this.salamiSliceRButton.UseVisualStyleBackColor = true;
            this.salamiSliceRButton.CheckedChanged += new System.EventHandler(this.salamiSliceRButton_CheckedChanged);
            // 
            // gardenOfEdenRButton
            // 
            this.gardenOfEdenRButton.AutoSize = true;
            this.gardenOfEdenRButton.Location = new System.Drawing.Point(35, 255);
            this.gardenOfEdenRButton.Name = "gardenOfEdenRButton";
            this.gardenOfEdenRButton.Size = new System.Drawing.Size(100, 17);
            this.gardenOfEdenRButton.TabIndex = 5;
            this.gardenOfEdenRButton.Text = "Garden of Eden";
            this.gardenOfEdenRButton.UseVisualStyleBackColor = true;
            this.gardenOfEdenRButton.CheckedChanged += new System.EventHandler(this.gardenOfEdenRButton_CheckedChanged);
            // 
            // russianDollRButton
            // 
            this.russianDollRButton.AutoSize = true;
            this.russianDollRButton.Location = new System.Drawing.Point(35, 198);
            this.russianDollRButton.Name = "russianDollRButton";
            this.russianDollRButton.Size = new System.Drawing.Size(84, 17);
            this.russianDollRButton.TabIndex = 6;
            this.russianDollRButton.Text = "Russian Doll";
            this.russianDollRButton.UseVisualStyleBackColor = true;
            this.russianDollRButton.CheckedChanged += new System.EventHandler(this.russianDollRButton_CheckedChanged);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(52, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(422, 26);
            this.label1.TabIndex = 7;
            this.label1.Text = "The resulting schema contains only one global element. Element declarations are n" +
                "ested\r\nwithin a single global declaration (complex type, group). Those types can" +
                " be reused.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(52, 161);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(409, 26);
            this.label2.TabIndex = 8;
            this.label2.Text = "All the elements are global. No nesting of element declarations is applied, decla" +
                "rations\r\ncan be reused in the schema. All elements are defined within the global" +
                " namespace.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(52, 218);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(380, 26);
            this.label4.TabIndex = 10;
            this.label4.Text = "There is only one global element. Element declaration are nested within a single\r" +
                "\nglobal declaration, which can be used once only.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(52, 275);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(380, 26);
            this.label3.TabIndex = 11;
            this.label3.Text = "There is only one global element. Element declaration are nested within a single\r" +
                "\nglobal declaration, which can be used once only.";
            // 
            // MidPage_DesignApproach
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.russianDollRButton);
            this.Controls.Add(this.gardenOfEdenRButton);
            this.Controls.Add(this.salamiSliceRButton);
            this.Controls.Add(this.venetianBlindRButton);
            this.Name = "MidPage_DesignApproach";
            this.Size = new System.Drawing.Size(490, 312);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.wizardBanner1, 0);
            this.Controls.SetChildIndex(this.venetianBlindRButton, 0);
            this.Controls.SetChildIndex(this.salamiSliceRButton, 0);
            this.Controls.SetChildIndex(this.gardenOfEdenRButton, 0);
            this.Controls.SetChildIndex(this.russianDollRButton, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label4, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton venetianBlindRButton;
        private System.Windows.Forms.RadioButton salamiSliceRButton;
        private System.Windows.Forms.RadioButton gardenOfEdenRButton;
        private System.Windows.Forms.RadioButton russianDollRButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;

    }
}
