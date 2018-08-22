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
    public partial class MidPage_FormattingOutput : ConfigWizardUI.InternalWizardPage
    {
        private bool emptyLineBeforeGlobal;
        private int cntIndentSpaces;

        public MidPage_FormattingOutput()
        {
            InitializeComponent();
            emptyLineBeforeGlobal = true;
            cntIndentSpaces = 4;
        }

        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }

        public bool isSetEmptyLineBeforeGlobal()
        {
            return emptyLineBeforeGlobal;
        }

        public int getIndentSpacesCnt()
        {
            return cntIndentSpaces;
        }

        private void emptyLineChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (emptyLineChkBox.Checked)
                emptyLineBeforeGlobal = true;
            else
                emptyLineBeforeGlobal = false;
        }

        private void cntIndentSpacesBox_TextChanged(object sender, EventArgs e)
        {
            string text = cntIndentSpacesBox.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(text, @"(^([0-9]*|\d*\d{1}?\d*)$)"))
            {
                // It is not a number!
                cntIndentSpacesBox.Text = null;
                
                return;
            }
            try
            {
                cntIndentSpaces = Convert.ToInt32(text);
            }
            catch (Exception)
            {
                // nothing to do
            }
        }
    }
}
