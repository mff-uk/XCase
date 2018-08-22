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
    public partial class WizardBanner : UserControl
    {
        public WizardBanner()
        {
            InitializeComponent();
        }

        [Category("Appearance")]
        public string Title
        {
            get { return titleLabel.Text; }
            set { titleLabel.Text = value; }
        }

        [Category("Appearance")]
        public string Subtitle
        {
            get { return subtitleLabel.Text; }
            set { subtitleLabel.Text = value; }
        }
    }
}
