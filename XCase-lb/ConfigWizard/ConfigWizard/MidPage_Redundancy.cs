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
    public partial class MidPage_Redundancy : ConfigWizardUI.InternalWizardPage
    {
        private bool isElimRedundantAPsEnabled;
        private bool isElimRedundantAttrDeclsEnabled;
        private bool isElimRedundancyInNestingsEnabled;

        public MidPage_Redundancy()
        {
            InitializeComponent();

            isElimRedundantAPsEnabled = false;
            isElimRedundantAttrDeclsEnabled = false;
            isElimRedundancyInNestingsEnabled = false;
        }

        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }

        private void elimRedAPsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (elimRedAPsChkBox.Checked)
                isElimRedundantAPsEnabled = true;
            else
                isElimRedundantAPsEnabled = false;
        }

        private void elimRedAttrDeclsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (elimRedAttrDeclsChkBox.Checked)
                isElimRedundantAttrDeclsEnabled = true;
            else
                isElimRedundantAttrDeclsEnabled = false;
        }

        private void elimRedInNestingsChkBox_CheckedChanged(object sender, EventArgs e)
        {
            if (elimRedInNestingsChkBox.Checked)
                isElimRedundancyInNestingsEnabled = true;
            else
                isElimRedundancyInNestingsEnabled = false;
        }

        public bool isElimRedAPsEnabled()
        {
            return isElimRedundantAPsEnabled;
        }

        public bool isElimRedAttrDeclsEnabled()
        {
            return isElimRedundantAttrDeclsEnabled;
        }

        public bool isElimRedInNestingsEnabled()
        {
            return isElimRedundancyInNestingsEnabled;
        }
    }
}
