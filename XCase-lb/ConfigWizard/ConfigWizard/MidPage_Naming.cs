using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ConfigWizardUI;

namespace ConfigWizard
{
    public partial class MidPage_Naming : ConfigWizardUI.InternalWizardPage
    {
        private bool includeProjName;
        private string groupMask;
        private string attrGroupMask;
        private int postfix;            // 0 .. numbers, 1 .. alphabet, 2 .. sequence
        private string sequenceText;    // comma delimited
        private string namespacePrefix;

        public MidPage_Naming()
        {
            InitializeComponent();
            includeProjName = false;
            postfix = 0;
            sequenceText = sequenceTextBox.Text;
            groupMask = groupMaskBox.Text;
            attrGroupMask = attrGroupMaskBox.Text;
            namespacePrefix = namespacePrefixBox.Text;
        }

        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }

        private void includeProjnameChkBox_CheckedChanged(object sender, EventArgs e)
        {
            includeProjName = includeProjnameChkBox.Checked;
        }

        public bool getIncludeProjName()
        {
            return includeProjName;
        }

        public string getGroupMask()
        {
            return groupMask;
        }

        public string getAttrGroupMask()
        {
            return attrGroupMask;
        }

        public int getPostfix()
        {
            return postfix;
        }

        public string getSequenceText()
        {
            return sequenceText;
        }

        public string getNamespacePrefix()
        {
            return namespacePrefix;
        }

        private void postfixNumbersRButton_CheckedChanged(object sender, EventArgs e)
        {
            postfix = 0; // numbers
            sequenceTextBox.Enabled = false;
        }

        private void postfixAlphabetRButton_CheckedChanged(object sender, EventArgs e)
        {
            postfix = 1; // alphabet
            sequenceTextBox.Enabled = false;
        }

        private void postfixSequenceRButton_CheckedChanged(object sender, EventArgs e)
        {
            postfix = 2; // sequence
            sequenceTextBox.Enabled = true;
        }

        private void sequenceTextBox_TextChanged(object sender, EventArgs e)
        {
            sequenceText = sequenceTextBox.Text;
        }

        private void groupMaskBox_TextChanged(object sender, EventArgs e)
        {
            groupMask = groupMaskBox.Text;
        }

        private void attrGroupMaskBox_TextChanged(object sender, EventArgs e)
        {
            attrGroupMask = attrGroupMaskBox.Text;
        }

        private void namespacePrefixBox_TextChanged(object sender, EventArgs e)
        {
            namespacePrefix = namespacePrefixBox.Text;
        }

        private void groupMaskBox_Leave(object sender, EventArgs e)
        {
            string value = groupMaskBox.Text;
            if (!value.Contains("%"))
            {
                MessageBox.Show("Group mask must contain the '%' character.\nIn group names of resulting schema it will be replaced by the name of related PSM class.", "Important Note",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                ActiveControl = groupMaskBox;
            }
        }

        private void attrGroupMaskBox_Leave(object sender, EventArgs e)
        {
            string value = attrGroupMaskBox.Text;
            if (!value.Contains("%"))
            {
                MessageBox.Show("Attribute group mask must contain the '%' character.\nIn attributeGroup names of resulting schema it will be replaced by the name of related PSM class.", "Important Note",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                ActiveControl = attrGroupMaskBox;
            }
        }

        private void namespacePrefixBox_Leave(object sender, EventArgs e)
        {
            string value = namespacePrefixBox.Text;
            try
            {
                Uri newURI = new Uri(value);
            }
            catch (Exception)
            {
                MessageBox.Show("The value you've entered is not a valid URI.", "Important Note",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                ActiveControl = namespacePrefixBox;
            }
        }
    }
}
