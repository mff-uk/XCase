using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ConfigWizardUI;

namespace ConfigWizard
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WizardSheet wizard = new WizardSheet();
            wizard.Pages.Add(new WelcomePage());

            MidPage_DesignApproach midpage_DA = new MidPage_DesignApproach();
            MidPage_UseNamespaces midpage_UN = new MidPage_UseNamespaces();
            MidPage_Redundancy midpage_R = new MidPage_Redundancy();
            CompletePage completePage = new CompletePage();

            wizard.Pages.Add(midpage_DA);
            wizard.Pages.Add(midpage_UN);
            wizard.Pages.Add(midpage_R);
            wizard.Pages.Add(completePage);

            Application.EnableVisualStyles();
            Application.Run(wizard);
        }
    }
}
