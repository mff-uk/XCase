using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConfigWizardUI
{
    public partial class WizardPage : UserControl
    {
        public WizardPage()
        {
            InitializeComponent();
        }

        [Category("Wizard")]
        public event CancelEventHandler SetActive;

        public virtual void OnSetActive(CancelEventArgs e)
        {
            if (SetActive != null)
                SetActive(this, e);
        }

        protected WizardSheet GetWizard()
        {
            WizardSheet wizard = (WizardSheet)this.ParentForm;
            return wizard;
        }

        protected void SetWizardButtons(WizardButtons buttons)
        {
            GetWizard().SetWizardButtons(buttons);
        }

        [Category("Wizard")]
        public event WizardPageEventHandler WizardNext;

        public virtual void OnWizardNext(WizardPageEventArgs e)
        {
            if (WizardNext != null)
                WizardNext(this, e);
        }

        [Category("Wizard")]
        public event WizardPageEventHandler WizardBack;

        public virtual void OnWizardBack(WizardPageEventArgs e)
        {
            if (WizardBack != null)
                WizardBack(this, e);
        }

        [Category("Wizard")]
        public event CancelEventHandler WizardFinish;

        public virtual void OnWizardFinish(CancelEventArgs e)
        {
            if (WizardFinish != null)
                WizardFinish(this, e);
        }

        [Category("Wizard")]
        public event CancelEventHandler QueryCancel;

        public virtual void OnQueryCancel(CancelEventArgs e)
        {
            if (QueryCancel != null)
                QueryCancel(this, e);
        }
    }
}
