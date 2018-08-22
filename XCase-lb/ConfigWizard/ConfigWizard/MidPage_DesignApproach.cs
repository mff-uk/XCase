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
    public partial class MidPage_DesignApproach : ConfigWizardUI.InternalWizardPage
    {
        string designApproach;

        public MidPage_DesignApproach()
        {
            this.InitializeComponent();
            designApproach = "venetian blind";
        }

        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }

        public string getDesignApproach()
        {
            return designApproach;
        }

        private void venetianBlindRButton_CheckedChanged(object sender, EventArgs e)
        {
            designApproach = "venetian blind";
        }

        private void salamiSliceRButton_CheckedChanged(object sender, EventArgs e)
        {
            designApproach = "salami slice";
        }

        private void gardenOfEdenRButton_CheckedChanged(object sender, EventArgs e)
        {
            designApproach = "garden of eden";
        }

        private void russianDollRButton_CheckedChanged(object sender, EventArgs e)
        {
            designApproach = "russian doll";
        }
    }
}
