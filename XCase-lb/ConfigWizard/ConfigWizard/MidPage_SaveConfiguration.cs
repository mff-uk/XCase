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
    public partial class MidPage_SaveConfiguration : ConfigWizardUI.InternalWizardPage
    {
        private string fileName;

        public MidPage_SaveConfiguration()
        {
            InitializeComponent();
        }

        private void MiddlePage_SetActive(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
        }

        public string getFileName()
        {
            return fileName;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckFileExists = false;
            sfd.CheckPathExists = false;
            sfd.Filter = "XML files (*.xml)|*.xml";
            sfd.InitialDirectory = Directory.GetCurrentDirectory();

            DialogResult dr = sfd.ShowDialog();
            if (dr != DialogResult.OK)
            {
                return;
            }

            SetWizardButtons(WizardButtons.Back | WizardButtons.Next);
            fileName = sfd.FileName;
            this.configFileBox.Text = fileName;
        }

        private void dontSaveChkBox_CheckedChanged(object sender, EventArgs e)
        {
            this.configFileBox.Enabled = !(dontSaveChkBox.Checked);
            this.browseButton.Enabled = !(dontSaveChkBox.Checked);
        }
    }
}
