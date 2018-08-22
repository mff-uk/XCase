namespace ConfigWizard
{
    partial class MidPage_WarningError
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.AbstractNotSpecializedCombo = new System.Windows.Forms.ComboBox();
            this.AssociationMultiplicityLostCombo = new System.Windows.Forms.ComboBox();
            this.AttributeMultiplicityLostCombo = new System.Windows.Forms.ComboBox();
            this.AttributesInChoiceCombo = new System.Windows.Forms.ComboBox();
            this.ClassNameEmptyCombo = new System.Windows.Forms.ComboBox();
            this.DuplicateRootElementsCombo = new System.Windows.Forms.ComboBox();
            this.ElementNameMissingCombo = new System.Windows.Forms.ComboBox();
            this.NoRootCombo = new System.Windows.Forms.ComboBox();
            this.NonAbstractClassCombo = new System.Windows.Forms.ComboBox();
            this.SpecializedAttributeGroupCombo = new System.Windows.Forms.ComboBox();
            this.TranslatedAttributeAliasCombo = new System.Windows.Forms.ComboBox();
            this.TranslatedClassNameCombo = new System.Windows.Forms.ComboBox();
            this.TypeTranslatedAsStringCombo = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // wizardBanner1
            // 
            this.wizardBanner1.Size = new System.Drawing.Size(513, 59);
            this.wizardBanner1.Subtitle = "Sometimes problems occurs during the translation. Choose the way of dealing with " +
                "them.";
            this.wizardBanner1.Title = "Warnings / Errors";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 76);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(291, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "There are no specializations of abstract class in the diagram.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 96);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(343, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Multiplicity can\'t be expressed - attr.group can be referenced only once.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(359, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Multiplicity of attribute can not be expressed - it is translated as an attribute" +
                ".";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(24, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(370, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Attrs under choice can\'t be translated as \"exclusive or\" - they will be optional." +
                "";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 156);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(127, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Name of a class is empty.";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(24, 176);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(293, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Root element name was changed to avoid duplicate names. ";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(24, 196);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(200, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Object must be named before translation.";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(24, 216);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(155, 13);
            this.label11.TabIndex = 11;
            this.label11.Text = "Schema has no global element.";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(24, 236);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(272, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "Class does not have an element label and is specialized.";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(24, 256);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(242, 13);
            this.label13.TabIndex = 13;
            this.label13.Text = "Specialized class does not have an element label.";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(24, 276);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(362, 13);
            this.label14.TabIndex = 14;
            this.label14.Text = "Alias of an attr must have been changed to be a valid name for an attribute.";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(24, 296);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(360, 13);
            this.label15.TabIndex = 15;
            this.label15.Text = "Name of a class must have been changed to be a valid for a complex type.";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(24, 316);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(281, 13);
            this.label16.TabIndex = 16;
            this.label16.Text = "Type of attribute is not specified, using xs:string as default.";
            // 
            // AbstractNotSpecializedCombo
            // 
            this.AbstractNotSpecializedCombo.FormattingEnabled = true;
            this.AbstractNotSpecializedCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.AbstractNotSpecializedCombo.Location = new System.Drawing.Point(407, 73);
            this.AbstractNotSpecializedCombo.Name = "AbstractNotSpecializedCombo";
            this.AbstractNotSpecializedCombo.Size = new System.Drawing.Size(82, 21);
            this.AbstractNotSpecializedCombo.TabIndex = 17;
            this.AbstractNotSpecializedCombo.SelectedValueChanged += new System.EventHandler(this.AbstractNotSpecializedCombo_SelectedValueChanged);
            // 
            // AssociationMultiplicityLostCombo
            // 
            this.AssociationMultiplicityLostCombo.FormattingEnabled = true;
            this.AssociationMultiplicityLostCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.AssociationMultiplicityLostCombo.Location = new System.Drawing.Point(407, 93);
            this.AssociationMultiplicityLostCombo.Name = "AssociationMultiplicityLostCombo";
            this.AssociationMultiplicityLostCombo.Size = new System.Drawing.Size(82, 21);
            this.AssociationMultiplicityLostCombo.TabIndex = 18;
            this.AssociationMultiplicityLostCombo.SelectedValueChanged += new System.EventHandler(this.AssociationMultiplicityLostCombo_SelectedValueChanged);
            // 
            // AttributeMultiplicityLostCombo
            // 
            this.AttributeMultiplicityLostCombo.FormattingEnabled = true;
            this.AttributeMultiplicityLostCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.AttributeMultiplicityLostCombo.Location = new System.Drawing.Point(407, 113);
            this.AttributeMultiplicityLostCombo.Name = "AttributeMultiplicityLostCombo";
            this.AttributeMultiplicityLostCombo.Size = new System.Drawing.Size(82, 21);
            this.AttributeMultiplicityLostCombo.TabIndex = 19;
            this.AttributeMultiplicityLostCombo.SelectedValueChanged += new System.EventHandler(this.AttributeMultiplicityLostCombo_SelectedValueChanged);
            // 
            // AttributesInChoiceCombo
            // 
            this.AttributesInChoiceCombo.FormattingEnabled = true;
            this.AttributesInChoiceCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.AttributesInChoiceCombo.Location = new System.Drawing.Point(407, 133);
            this.AttributesInChoiceCombo.Name = "AttributesInChoiceCombo";
            this.AttributesInChoiceCombo.Size = new System.Drawing.Size(82, 21);
            this.AttributesInChoiceCombo.TabIndex = 21;
            this.AttributesInChoiceCombo.SelectedValueChanged += new System.EventHandler(this.AttributesInChoiceCombo_SelectedValueChanged);
            // 
            // ClassNameEmptyCombo
            // 
            this.ClassNameEmptyCombo.FormattingEnabled = true;
            this.ClassNameEmptyCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.ClassNameEmptyCombo.Location = new System.Drawing.Point(407, 153);
            this.ClassNameEmptyCombo.Name = "ClassNameEmptyCombo";
            this.ClassNameEmptyCombo.Size = new System.Drawing.Size(82, 21);
            this.ClassNameEmptyCombo.TabIndex = 22;
            this.ClassNameEmptyCombo.SelectedValueChanged += new System.EventHandler(this.ClassNameEmptyCombo_SelectedValueChanged);
            // 
            // DuplicateRootElementsCombo
            // 
            this.DuplicateRootElementsCombo.FormattingEnabled = true;
            this.DuplicateRootElementsCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.DuplicateRootElementsCombo.Location = new System.Drawing.Point(407, 173);
            this.DuplicateRootElementsCombo.Name = "DuplicateRootElementsCombo";
            this.DuplicateRootElementsCombo.Size = new System.Drawing.Size(82, 21);
            this.DuplicateRootElementsCombo.TabIndex = 23;
            this.DuplicateRootElementsCombo.SelectedValueChanged += new System.EventHandler(this.DuplicateRootElementsCombo_SelectedValueChanged);
            // 
            // ElementNameMissingCombo
            // 
            this.ElementNameMissingCombo.FormattingEnabled = true;
            this.ElementNameMissingCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.ElementNameMissingCombo.Location = new System.Drawing.Point(407, 193);
            this.ElementNameMissingCombo.Name = "ElementNameMissingCombo";
            this.ElementNameMissingCombo.Size = new System.Drawing.Size(82, 21);
            this.ElementNameMissingCombo.TabIndex = 24;
            this.ElementNameMissingCombo.SelectedValueChanged += new System.EventHandler(this.ElementNameMissingCombo_SelectedValueChanged);
            // 
            // NoRootCombo
            // 
            this.NoRootCombo.FormattingEnabled = true;
            this.NoRootCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.NoRootCombo.Location = new System.Drawing.Point(407, 213);
            this.NoRootCombo.Name = "NoRootCombo";
            this.NoRootCombo.Size = new System.Drawing.Size(82, 21);
            this.NoRootCombo.TabIndex = 26;
            this.NoRootCombo.SelectedValueChanged += new System.EventHandler(this.NoRootCombo_SelectedValueChanged);
            // 
            // NonAbstractClassCombo
            // 
            this.NonAbstractClassCombo.FormattingEnabled = true;
            this.NonAbstractClassCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.NonAbstractClassCombo.Location = new System.Drawing.Point(407, 233);
            this.NonAbstractClassCombo.Name = "NonAbstractClassCombo";
            this.NonAbstractClassCombo.Size = new System.Drawing.Size(82, 21);
            this.NonAbstractClassCombo.TabIndex = 27;
            this.NonAbstractClassCombo.SelectedValueChanged += new System.EventHandler(this.NonAbstractClassCombo_SelectedValueChanged);
            // 
            // SpecializedAttributeGroupCombo
            // 
            this.SpecializedAttributeGroupCombo.FormattingEnabled = true;
            this.SpecializedAttributeGroupCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.SpecializedAttributeGroupCombo.Location = new System.Drawing.Point(407, 253);
            this.SpecializedAttributeGroupCombo.Name = "SpecializedAttributeGroupCombo";
            this.SpecializedAttributeGroupCombo.Size = new System.Drawing.Size(82, 21);
            this.SpecializedAttributeGroupCombo.TabIndex = 28;
            this.SpecializedAttributeGroupCombo.SelectedValueChanged += new System.EventHandler(this.SpecializedAttributeGroupCombo_SelectedValueChanged);
            // 
            // TranslatedAttributeAliasCombo
            // 
            this.TranslatedAttributeAliasCombo.FormattingEnabled = true;
            this.TranslatedAttributeAliasCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.TranslatedAttributeAliasCombo.Location = new System.Drawing.Point(407, 273);
            this.TranslatedAttributeAliasCombo.Name = "TranslatedAttributeAliasCombo";
            this.TranslatedAttributeAliasCombo.Size = new System.Drawing.Size(82, 21);
            this.TranslatedAttributeAliasCombo.TabIndex = 29;
            this.TranslatedAttributeAliasCombo.SelectedValueChanged += new System.EventHandler(this.TranslatedAttributeAliasCombo_SelectedValueChanged);
            // 
            // TranslatedClassNameCombo
            // 
            this.TranslatedClassNameCombo.FormattingEnabled = true;
            this.TranslatedClassNameCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.TranslatedClassNameCombo.Location = new System.Drawing.Point(407, 293);
            this.TranslatedClassNameCombo.Name = "TranslatedClassNameCombo";
            this.TranslatedClassNameCombo.Size = new System.Drawing.Size(82, 21);
            this.TranslatedClassNameCombo.TabIndex = 30;
            this.TranslatedClassNameCombo.SelectedValueChanged += new System.EventHandler(this.TranslatedClassNameCombo_SelectedValueChanged);
            // 
            // TypeTranslatedAsStringCombo
            // 
            this.TypeTranslatedAsStringCombo.FormattingEnabled = true;
            this.TypeTranslatedAsStringCombo.Items.AddRange(new object[] {
            "Ignore",
            "Warning",
            "Error"});
            this.TypeTranslatedAsStringCombo.Location = new System.Drawing.Point(407, 313);
            this.TypeTranslatedAsStringCombo.Name = "TypeTranslatedAsStringCombo";
            this.TypeTranslatedAsStringCombo.Size = new System.Drawing.Size(82, 21);
            this.TypeTranslatedAsStringCombo.TabIndex = 31;
            this.TypeTranslatedAsStringCombo.SelectedValueChanged += new System.EventHandler(this.TypeTranslatedAsStringCombo_SelectedValueChanged);
            // 
            // MidPage_WarningError
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.TypeTranslatedAsStringCombo);
            this.Controls.Add(this.TranslatedClassNameCombo);
            this.Controls.Add(this.TranslatedAttributeAliasCombo);
            this.Controls.Add(this.SpecializedAttributeGroupCombo);
            this.Controls.Add(this.NonAbstractClassCombo);
            this.Controls.Add(this.NoRootCombo);
            this.Controls.Add(this.ElementNameMissingCombo);
            this.Controls.Add(this.DuplicateRootElementsCombo);
            this.Controls.Add(this.ClassNameEmptyCombo);
            this.Controls.Add(this.AttributesInChoiceCombo);
            this.Controls.Add(this.AttributeMultiplicityLostCombo);
            this.Controls.Add(this.AssociationMultiplicityLostCombo);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.AbstractNotSpecializedCombo);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Name = "MidPage_WarningError";
            this.Size = new System.Drawing.Size(513, 345);
            this.SetActive += new System.ComponentModel.CancelEventHandler(this.MiddlePage_SetActive);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.label6, 0);
            this.Controls.SetChildIndex(this.label7, 0);
            this.Controls.SetChildIndex(this.label8, 0);
            this.Controls.SetChildIndex(this.label11, 0);
            this.Controls.SetChildIndex(this.label12, 0);
            this.Controls.SetChildIndex(this.label13, 0);
            this.Controls.SetChildIndex(this.AbstractNotSpecializedCombo, 0);
            this.Controls.SetChildIndex(this.label14, 0);
            this.Controls.SetChildIndex(this.label15, 0);
            this.Controls.SetChildIndex(this.label16, 0);
            this.Controls.SetChildIndex(this.wizardBanner1, 0);
            this.Controls.SetChildIndex(this.AssociationMultiplicityLostCombo, 0);
            this.Controls.SetChildIndex(this.AttributeMultiplicityLostCombo, 0);
            this.Controls.SetChildIndex(this.AttributesInChoiceCombo, 0);
            this.Controls.SetChildIndex(this.ClassNameEmptyCombo, 0);
            this.Controls.SetChildIndex(this.DuplicateRootElementsCombo, 0);
            this.Controls.SetChildIndex(this.ElementNameMissingCombo, 0);
            this.Controls.SetChildIndex(this.NoRootCombo, 0);
            this.Controls.SetChildIndex(this.NonAbstractClassCombo, 0);
            this.Controls.SetChildIndex(this.SpecializedAttributeGroupCombo, 0);
            this.Controls.SetChildIndex(this.TranslatedAttributeAliasCombo, 0);
            this.Controls.SetChildIndex(this.TranslatedClassNameCombo, 0);
            this.Controls.SetChildIndex(this.TypeTranslatedAsStringCombo, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox AbstractNotSpecializedCombo;
        private System.Windows.Forms.ComboBox AssociationMultiplicityLostCombo;
        private System.Windows.Forms.ComboBox AttributeMultiplicityLostCombo;
        private System.Windows.Forms.ComboBox AttributesInChoiceCombo;
        private System.Windows.Forms.ComboBox ClassNameEmptyCombo;
        private System.Windows.Forms.ComboBox DuplicateRootElementsCombo;
        private System.Windows.Forms.ComboBox ElementNameMissingCombo;
        private System.Windows.Forms.ComboBox NoRootCombo;
        private System.Windows.Forms.ComboBox NonAbstractClassCombo;
        private System.Windows.Forms.ComboBox SpecializedAttributeGroupCombo;
        private System.Windows.Forms.ComboBox TranslatedAttributeAliasCombo;
        private System.Windows.Forms.ComboBox TranslatedClassNameCombo;
        private System.Windows.Forms.ComboBox TypeTranslatedAsStringCombo;



    }
}
