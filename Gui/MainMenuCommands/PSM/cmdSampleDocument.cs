using System.Windows.Controls;
using XCase.Gui.Windows;
using XCase.Model;
using XCase.Translation.XmlSchema;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Opens XML schema window, initiates translation of the current diagram
	/// </summary>
	public class cmdXmlSchema: MainMenuCommandBase
	{
		public cmdXmlSchema(MainWindow mainWindow, Control control) : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
		}

		public XMLSchemaWindow Window { get; private set; }

		public override void Execute(object parameter)
		{
			XmlSchemaTranslator translator = new XmlSchemaTranslator();
			string schema = translator.Translate((PSMDiagram)ActiveDiagramView.Diagram);

		    XMLSchemaWindow.Show(MainWindow.dockManager, (PSMDiagram) ActiveDiagramView.Diagram, schema, translator.Log);
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.Diagram != null && ActiveDiagramView.Diagram is PSMDiagram;
		}
	}
}