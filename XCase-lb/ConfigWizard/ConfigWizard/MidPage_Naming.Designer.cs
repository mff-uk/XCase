namespace ConfigWizard
{
    partial class MidPage_Naming
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
            this.label1 = new System.Windows.Forms.Label();
            this.postfixNumbersRButton = new System.Windows.Forms.RadioButton();
            this.postfixAlphabetRButton = new System.Windows.Forms.RadioButton();
            this.postfixSequenceRButton = new System.Windows.Forms.RadioButton();
            this.sequenceTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupMaskBox = new System.Windows.Forms.TextBox();
            this.attrGroupMaskBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.namespacePrefixBox = new System.Windows.Forms.TextBox();
            this.includeProjnameChkBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.Size = new System.Drawing.Size(482, 59);
            this.wizardBanner1.Subtitle = "You can affect the way of naming objects of resulting schema.";
            this.wizardBanner1.Title = "Schema object names";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(390, 26);
            this.label1.TabIndex = 1;
            this.label1.Text = "There can be two and more PSM classes with equal names in your PSM diagram.\r\nWhic" +
                "h postfix should be used to distinguish them in the resulting XML Schema?";
            // 
            // postfixNumbersRButton
            // 
            this.postfixNumbersRButton.AutoSize = true;
            this.postfixNumbersRButton.Checked = true;
            this.postfixNumbersRButton.Location = new System.Drawing.Point(45, 120);
            this.postfixNumbersRButton.Name = "postfixNumbersRButton";
            this.postfixNumbersRButton.Size = new System.Drawing.Size(65, 17);
            this.postfixNumbersRButton.TabIndex = 2;
            this.postfixNumbersRButton.TabStop = true;
            this.postfixNumbersRButton.Text = "numbers";
            this.postfixNumbersRButton.UseVisualStyleBackColor = true;
            this.postfixNumbersRButton.CheckedChanged += new System.EventHandler(this.postfixNumbersRButton_CheckedChanged);
            // 
            // postfixAlphabetRButton
            // 
            this.postfixAlphabetRButton.AutoSize = true;
            this.postfixAlphabetRButton.Location = new System.Drawing.Point(116, 119);
            this.postfixAlphabetRButton.Name = "postfixAlphabetRButton";
            this.postfixAlphabetRButton.Size = new System.Drawing.Size(66, 17);
            this.postfixAlphabetRButton.TabIndex = 3;
            this.postfixAlphabetRButton.Text = "alphabet";
            this.postfixAlphabetRButton.UseVisualStyleBackColor = true;
            this.postfixAlphabetRButton.CheckedChanged += new System.EventHandler(this.postfixAlphabetRButton_CheckedChanged);
            // 
            // postfixSequenceRButton
            // 
            this.postfixSequenceRButton.AutoSize = true;
            this.postfixSequenceRButton.Location = new System.Drawing.Point(188, 119);
            this.postfixSequenceRButton.Name = "postfixSequenceRButton";
            this.postfixSequenceRButton.Size = new System.Drawing.Size(72, 17);
            this.postfixSequenceRButton.TabIndex = 4;
            this.postfixSequenceRButton.Text = "sequence";
            this.postfixSequenceRButton.UseVisualStyleBackColor = true;
            this.postfixSequenceRButton.CheckedChanged += new System.EventHandler(this.postfixSequenceRButton_CheckedChanged);
            // 
            // sequenceTextBox
            // 
            this.sequenceTextBox.Enabled = false;
            this.sequenceTextBox.Location = new System.Drawing.Point(266, 119);
            this.sequenceTextBox.Name = "sequenceTextBox";
            this.sequenceTextBox.Size = new System.Drawing.Size(186, 20);
            this.sequenceTextBox.TabIndex = 5;
            this.sequenceTextBox.Text = "comma separated";
            this.sequenceTextBox.TextChanged += new System.EventHandler(this.sequenceTextBox_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(35, 165);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(410, 26);
            this.label2.TabIndex = 6;
            this.label2.Text = "PSM class names can result in XML Schema group names and attributeGroup names.\r\nS" +
                "pecify masks of those names using % in place of the PSM class name.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(232, 204);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "group";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(232, 222);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "attributeGroup";
            // 
            // groupMaskBox
            // 
            this.groupMaskBox.Location = new System.Drawing.Point(311, 201);
            this.groupMaskBox.Name = "groupMaskBox";
            this.groupMaskBox.Size = new System.Drawing.Size(141, 20);
            this.groupMaskBox.TabIndex = 9;
            this.groupMaskBox.Text = "%-c";
            this.groupMaskBox.TextChanged += new System.EventHandler(this.groupMaskBox_TextChanged);
            this.groupMaskBox.Leave += new System.EventHandler(this.groupMaskBox_Leave);
            // 
            // attrGroupMaskBox
            // 
            this.attrGroupMaskBox.Location = new System.Drawing.Point(311, 219);
            this.attrGroupMaskBox.Name = "attrGroupMaskBox";
            this.attrGroupMaskBox.Size = new System.Drawing.Size(141, 20);
            this.attrGroupMaskBox.TabIndex = 10;
            this.attrGroupMaskBox.Text = "%-a";
            this.attrGroupMaskBox.TextChanged += new System.EventHandler(this.attrGroupMaskBox_TextChanged);
            this.attrGroupMaskBox.Leave += new System.EventHandler(this.attrGroupMaskBox_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(35, 262);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(170, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Prefix for XML Namespace values:";
            // 
            // namespacePrefixBox
            // 
            this.namespacePrefixBox.Location = new System.Drawing.Point(211, 259);
            this.namespacePrefixBox.Name = "namespacePrefixBox";
            this.namespacePrefixBox.Size = new System.Drawing.Size(241, 20);
            this.namespacePrefixBox.TabIndex = 12;
            this.namespacePrefixBox.Text = "http://kocour.ms.mff.cuni.cz/xcase/";
            this.namespacePrefixBox.TextChanged += new System.EventHandler(this.namespacePrefixBox_TextChanged);
            this.namespacePrefixBox.Leave += new System.EventHandler(this.namespacePrefixBox_Leave);
            // 
            // includeProjnameChkBox
            // 
            this.includeProjnameChkBox.AutoSize = true;
            this.includeProjnameChkBox.Location = new System.Drawing.Point(38, 284);
            this.includeProjnameChkBox.Name = "includeProjnameChkBox";
            this.includeProjnameChkBox.Size = new System.Drawing.Size(311, 17);
            this.includeProjnameChkBox.TabIndex = 13;
            this.includeProjnameChkBox.Text = "include name of current project into XML Namespace values";
            this.includeProjnameChkBox.UseVisualStyleBackColor = true;
            this.includeProjnameChkBox.CheckedChanged += new System.EventHandler(this.includeProjnameChkBox_CheckedChanged);
            // 
            // MidPage_Naming
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.includeProjnameChkBox);
            this.Controls.Add(this.namespacePrefixBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.attrGroupMaskBox);
            this.Controls.Add(this.groupMaskBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.postfixSequenceRButton);
            this.Controls.Add(this.postfixNumbersRButton);
            this.Controls.Add(this.sequenceTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.postfixAlphabetRButton);
            this.Name = "MidPage_Naming";
            this.Size = new System.Drawing.Size(482, 345);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.postfixAlphabetRButton, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.sequenceTextBox, 0);
            this.Controls.SetChildIndex(this.postfixNumbersRButton, 0);
            this.Controls.SetChildIndex(this.postfixSequenceRButton, 0);
            this.Controls.SetChildIndex(this.wizardBanner1, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.label4, 0);
            this.Controls.SetChildIndex(this.groupMaskBox, 0);
            this.Controls.SetChildIndex(this.attrGroupMaskBox, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.namespacePrefixBox, 0);
            this.Controls.SetChildIndex(this.includeProjnameChkBox, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton postfixNumbersRButton;
        private System.Windows.Forms.RadioButton postfixAlphabetRButton;
        private System.Windows.Forms.RadioButton postfixSequenceRButton;
        private System.Windows.Forms.TextBox sequenceTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox groupMaskBox;
        private System.Windows.Forms.TextBox attrGroupMaskBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox namespacePrefixBox;
        private System.Windows.Forms.CheckBox includeProjnameChkBox;



    }
}
