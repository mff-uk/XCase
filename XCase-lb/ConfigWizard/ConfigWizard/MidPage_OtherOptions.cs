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
    public partial class MidPage_OtherOptions : ConfigWizardUI.InternalWizardPage
    {
        private bool useNamespaces;

        public MidPage_OtherOptions()
        {
            InitializeComponent();
        }

        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }
    }
}
