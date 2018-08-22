using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace XCase.Translation.XmlSchema
{
    public partial class StartTranslation : Form
    {
        private bool defConfiguration;

        public StartTranslation()
        {
            InitializeComponent();
            this.configFileBox.Text = "";
            defConfiguration = true;
        }

        public string getConfigFileName()
        {
            return this.configFileBox.Text;
        }

        public bool isDefConfigChecked()
        {
            return defConfiguration;
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.Filter = "XML files (*.xml)|*.xml";

            string text = this.configFileBox.Text;
            if ((text != null) && (text != ""))
            {
                FileInfo finfo = new FileInfo(text);
                ofd.InitialDirectory = finfo.DirectoryName;
            }

            DialogResult dr = ofd.ShowDialog();
            if (dr != DialogResult.OK)
            {
                return;
            }

            this.configFileBox.Text = ofd.FileName;
            this.OKButton.Enabled = true;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void useConfigRButton_CheckedChanged(object sender, EventArgs e)
        {
            this.browseButton.Enabled = true;
            this.configFileBox.Enabled = true;
            defConfiguration = false;
            if (this.configFileBox.TextLength < 1)
            {
                this.OKButton.Enabled = false;
            }
        }

        private void createConfigRButton_CheckedChanged(object sender, EventArgs e)
        {
            this.browseButton.Enabled = false;
            this.configFileBox.Enabled = false;
            defConfiguration = true;
            this.OKButton.Enabled = true;
        }

        private void newConfigButton_Click(object sender, EventArgs e)
        {
            string fileName = Configuration.createWithWizard();
            this.configFileBox.Text = fileName;
            this.OKButton.Enabled = true;
            if (fileName != null)
            {
                this.useConfigRButton.Checked = true;
            }
        }
    }
}
