using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

using ConfigWizardUI;

namespace ConfigWizard
{
    public partial class CompletePage : ConfigWizardUI.ExternalWizardPage
    {
        public CompletePage()
        {
            InitializeComponent();
            int i=0;
            string fileName = "";

            do {
                fileName = GetHomeDir() + "\\profile_" + (++i) + ".xml";
            } while (File.Exists(fileName));

            this.configFileBox.Text = fileName;
        }

        private void CompletePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Finish);
        }

        private static String GetHomeDir()
        {
            //return Environment.GetEnvironmentVariable("USERPROFILE");
            return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = false;
            sfd.Filter = "XML files (*.xml)|*.xml";
            sfd.InitialDirectory = GetHomeDir();

            DialogResult dr = sfd.ShowDialog();
            if (dr != DialogResult.OK)
            {
                return;
            }

            //SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
            this.configFileBox.Text = sfd.FileName;
        }

        public string getFileName()
        {
            return this.configFileBox.Text;
        }
    }
}
