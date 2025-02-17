﻿using System;
using System.ComponentModel;

namespace ConfigWizardUI
{
    public class WizardPageEventArgs : CancelEventArgs
    {
        string _newPage = null;

        public string NewPage
        {
            get { return _newPage; }
            set { _newPage = value; }
        }
    }

    public delegate void WizardPageEventHandler(object sender, WizardPageEventArgs e);
}