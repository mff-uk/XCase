using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml;
using System.Xml.Schema;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using DDialogResult = System.Windows.Forms.DialogResult;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;

namespace ProjectConverter
{
    /// <summary>
    /// Interaction logic for fProjectConverter.xaml
    /// </summary>
    public partial class fProjectConverter
    {
        public fProjectConverter()
        {
            InitializeComponent();
        }

        public void UpgradeDirectory(string directory, bool showResultWindow)
        {
            DirectoryInfo di = new DirectoryInfo(directory);
            UpgradeDirectory(di, showResultWindow);
        }

        public bool UpgradeFile(string file, bool showErrors)
        {
            FileInfo fi = new FileInfo(file);
            return UpgradeFile(fi, cbBackup.IsChecked.Value, showErrors);
        }

        public void UpgradeDirectory(DirectoryInfo directory, bool showResultWindow)
        {
            if (!directory.Exists)
            {
                ProcessError(string.Format("Directory {0} does not exist. ", directory), "Directory does not exist.", showResultWindow);
                return;
            }

            FileInfo[] files = directory.GetFiles("*.XCase", cbSubdirectories.IsChecked.Value ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            foreach (FileInfo file in files)
            {
                bool result = UpgradeFile(file, cbBackup.IsChecked.Value, false);
                if (result)
                    log.AppendLine(string.Format("Upgraded file: {0}.", file));
            }

            if (log.Length > 0)
            {
                if (showResultWindow)
                    ShowResult(log);
            }
        }

        private static void ShowResult(StringBuilder log)
        {
            MessageBox.Show(log.ToString(), "Result", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private readonly StringBuilder log = new StringBuilder();

        public bool UpgradeFile(FileInfo file, bool createBackup, bool showErrors)
        {
            #region errorcheck
            if (!file.Exists)
            {
                ProcessError(string.Format("File {0} does not exist. ", file), "File does not exist.", showErrors);
                return false;
            }
            if (file.IsReadOnly)
            {
                ProcessError(string.Format("File {0} can not be processed, it is read only. ", file), "File is read only.", showErrors);
                return false;
            }

            string error = null;
            if (!Validate(file.FullName, ref error))
            {
                ProcessError(string.Format("Project file {0} is not valid XCase project document version 1. \r\n{1}", file, error), "Invalid file", showErrors);
                return false;
            }

            #endregion

            if (createBackup)
            {
                string backupPath = file.FullName + ".bak";
                using (StreamReader reader = file.OpenText())
                {
                    File.WriteAllText(backupPath, reader.ReadToEnd(), Encoding.UTF8);
                }
                log.AppendLine(string.Format("Created backup file: {0}.", backupPath));
            }

            ProjectConverterV1V2 c = new ProjectConverterV1V2();

            try
            {
                c.ConvertV1V2(file);
            }
            catch (Exception e)
            {
                log.AppendFormat("Can not upgrade file : {0}. Reason: \r\n\r\n {1}\r\n\r\n", file.FullName, e.Message);
                return false; 
            }
            return true; 
        }

        #region xml validation

        protected static String xmlValidationErrorMessage;

        protected static bool isPassedXmlValid = true;

        protected static void schemaSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            isPassedXmlValid = false;
            xmlValidationErrorMessage = e.Message;
        }

        public bool ValidateXML(Stream input, ref String message)
        {
            return Validate(input, ref message);
        }

        public bool ValidateXML(string file, ref String message)
        {
            return Validate(file, ref message);
        }

        public virtual string defaultNamespace { get { return "http://kocour.ms.mff.cuni.cz/~necasky/xcase"; } }

        public bool DialogMode { get; set; }

        protected bool Validate(object input, ref String message)
        {
            // TODO: it would be better to have two separate schemas rather than one choice schema 

            // Load XML Schema file describing the correct XML file
            byte[] b = Encoding.GetEncoding("windows-1250").GetBytes(Properties.Resources.XCaseSchemaV1);
            MemoryStream m = new MemoryStream(b);
            XmlReader schema = new XmlTextReader(m);

            XmlReaderSettings schemaSettings = new XmlReaderSettings();
            schemaSettings.Schemas.Add(defaultNamespace, schema);
            schemaSettings.ValidationType = ValidationType.Schema;

            // Event handler called when an error occurs while validating
            schemaSettings.ValidationEventHandler += schemaSettings_ValidationEventHandler;

            XmlReader xmlfile;
            if (input is string)
                xmlfile = XmlReader.Create((string)input, schemaSettings);
            else
                if (input is Stream)
                    xmlfile = XmlReader.Create((Stream)input, schemaSettings);
                else
                    return false;

            try
            {
                while (xmlfile.Read())
                {
                }
            }
            catch
            {
                isPassedXmlValid = false;
                xmlValidationErrorMessage = "Not a valid XCase file v1";
            }
            finally
            {
                xmlfile.Close();
                schema.Close();
                m.Dispose();
                xmlfile.Close();
            }

            if (isPassedXmlValid)
                return true;
            else
            {
                message += xmlValidationErrorMessage;
                isPassedXmlValid = true;
                return false;
            }
        }

        #endregion

        private void ProcessError(string text, string title, bool showErrors)
        {
            if (showErrors)
                MessageBox.Show(text, title, MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                log.AppendLine(text);
                log.AppendLine();
            }
        }

        public void SetFile(string file)
        {
            rbProject.IsChecked = true;
            tbProject.Text = file;
        }

        public bool CanConvert(string v1, string v2)
        {
            return v1 == "1.0" && v2 == "2.0";
        }

        private void rbProject_Checked(object sender, RoutedEventArgs e)
        {
            if (tbDirectory != null)
            {
                tbDirectory.IsEnabled = rbDir.IsChecked.Value;
                bOpenDir.IsEnabled = rbDir.IsChecked.Value;
                cbSubdirectories.IsEnabled = rbDir.IsChecked.Value;

                tbProject.IsEnabled = rbProject.IsChecked.Value;
                bOpenFile.IsEnabled = rbProject.IsChecked.Value;
            }
        }

        private void rbDir_Checked(object sender, RoutedEventArgs e)
        {
            tbDirectory.IsEnabled = rbDir.IsChecked.Value;
            bOpenDir.IsEnabled = rbDir.IsChecked.Value;
            cbSubdirectories.IsEnabled = rbDir.IsChecked.Value;

            tbProject.IsEnabled = rbProject.IsChecked.Value;
            bOpenFile.IsEnabled = rbProject.IsChecked.Value;
        }

        private void bUpgrade_Click(object sender, RoutedEventArgs e)
        {
            if (log.Length > 0)
                log.Remove(0, log.Length - 1);
            if (rbProject.IsChecked.Value && !String.IsNullOrEmpty(tbProject.Text))
            {
                UpgradeFile(tbProject.Text, true);
            }
            if (rbDir.IsChecked.Value && !String.IsNullOrEmpty(tbDirectory.Text))
            {
                UpgradeDirectory(tbDirectory.Text, true);
            }

            if (DialogMode)
                Close();
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void bOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
                                                {
                                                    DefaultExt = "*.XCase",
                                                    Filter = "XCase projects (*.XCase)|*.XCase|XML files (*.xml)|*.xml|All files|*.*",
                                                    Title = "Open project "
                                                };

            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                tbProject.Text = openFileDialog.FileName;
            }
        }

        private void bOpenDir_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog openFolderDialog
                = new FolderBrowserDialog
                      {
                          Description = "Select folder",
                          RootFolder = Environment.SpecialFolder.MyComputer,
                          ShowNewFolderButton = false
                      };

            DDialogResult result = openFolderDialog.ShowDialog();
            if (result == DDialogResult.OK)
            {
                tbDirectory.Text = openFolderDialog.SelectedPath;
            }
        }
    }
}
