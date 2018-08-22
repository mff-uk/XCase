using XCase.Gui.Windows;
using XCase.Model;
using XCase.Translation.XmlSchema;
using XCase.Translation;
using XCase.Translation.DataGenerator;
using System.Collections.Generic;
using System.Windows.Forms;
using ConfigWizardUI;
using ConfigWizard;
using System.IO;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Opens XML schema window, initiates translation of the current diagram
	/// </summary>
	public class cmdXmlSchema: MainMenuCommandBase
	{
		public cmdXmlSchema(MainWindow mainWindow, System.Windows.Controls.Control control) : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
		}

        public XMLSchemaWindow Window { get; private set; }

		public override void Execute(object parameter)
		{
            // todo: uncomment the next statement to run a test
            //Test(); return;

            PSMDiagram diagram = (PSMDiagram)ActiveDiagramView.Diagram;
            if (diagram.Roots.Count < 1)
            {
                MessageBox.Show("PSM diagram is empty. Nothing to translate.","XCase Warning");
                return;
            }

            // show dialog starting translation
            StartTranslation st = new StartTranslation();
            DialogResult dr = st.ShowDialog();
            if (dr != DialogResult.OK)
                return;

            Configuration config = new Configuration();
            if (!st.isDefConfigChecked())
            {
                config.Load(st.getConfigFileName());
            }

            if (config == null)
                return;

            // get short name of current project
            string projectName = getProjectName();

            // call XML Schema translation
            XmlSchemaTranslator translator = new XmlSchemaTranslator(config, projectName);
			string resultMessage = translator.Translate(diagram);

            if (resultMessage.Equals("ok"))
            {
                // get results and display them in nice window
                Dictionary<string,string> schemas = translator.getResults();
                XMLSchemaWindow.Show(MainWindow.dockManager, (PSMDiagram)ActiveDiagramView.Diagram, schemas, translator.Log);
            }
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.Diagram != null && ActiveDiagramView.Diagram is PSMDiagram;
		}

        public string getProjectName()
        {
            string projectName = MainWindow.Title;
            int ixWhitespace = projectName.LastIndexOf(" ");
            if (ixWhitespace < 0)
                return projectName;

            projectName = projectName.Substring(ixWhitespace + 1);
            if (projectName[projectName.Length - 1] == '*')
                projectName = projectName.Substring(0, projectName.Length - 1);
            return projectName;
        }

        // TODO: for testing purposes only
        // This function gets configDirectoryName - path to directory in which some
        // configuration files can be found. Using these config files this function will
        // generate XSD schemas from active PSM diagram and save them to path
        // specified by outputDirectoryName.
        private void Test()
        {
            string configDirectoryName = "D:/config";
            string outputDirectoryName = "D:/out/"+getCurrentTimeString();

            // list all configuration files presented in configDirectoryName
            DirectoryInfo di = new DirectoryInfo(configDirectoryName);
            FileInfo[] rgFiles = di.GetFiles("*.xml");
            foreach (FileInfo fi in rgFiles)
            {
                Configuration config = new Configuration();
                config.Load(fi.FullName);

                if (config == null)
                    continue;     // configuration file is probably invalid

                // create directory for output of the next translation
                DirectoryInfo diOutput = new DirectoryInfo(outputDirectoryName + "/" + fi.Name);
                diOutput.Create();

                // call translation and save results
                GenerateXsd(config, diOutput);
            }
        }

        // TODO: for testing purposes only
        // This function calls translation of active PSM diagram into XML Schema schemas.
        // It stores resulting schemas (.xsd files) to the path specified by diOutput parameter.
        private void GenerateXsd(Configuration config, DirectoryInfo diOutput)
        {
            // get short name of current project
            string projectName = getProjectName();

            // call XML Schema translation
            XmlSchemaTranslator translator = new XmlSchemaTranslator(config, projectName);
            string resultMessage = translator.Translate((PSMDiagram)ActiveDiagramView.Diagram);

            if (resultMessage.Equals("ok"))
            {
                // get results and store them into the output directory
                Dictionary<string, string> schemas = translator.getResults();
                foreach (string fileName in schemas.Keys)
                {
                    File.WriteAllText(diOutput.FullName + "/" + fileName, schemas[fileName], System.Text.Encoding.UTF8);
                }
            }
        }

        // TODO: for testing purposes only
        private string getCurrentTimeString()
        {
            System.DateTime dt = System.DateTime.Now;
            return dt.ToString("MM-dd HH-mm-ss");
        }
	}
}