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
    public partial class MidPage_UseNamespaces : ConfigWizardUI.InternalWizardPage
    {
        private bool useNamespaces;

        public MidPage_UseNamespaces()
        {
            InitializeComponent();
            useNamespaces = true;
        }

        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }

        public bool getUseNamespaces()
        {
            return useNamespaces;
        }

        private void noRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            useNamespaces = false;
        }

        private void yesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            useNamespaces = true;
        }
    }
}
