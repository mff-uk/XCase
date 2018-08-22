using System.Windows.Controls;
using XCase.Gui.Windows;
using XCase.Model;
using XCase.Translation.DataGenerator;
using XCase.Translation.XmlSchema;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Opens Sample document window, generates sample document
	/// </summary>
	public class cmdSampleDocument: MainMenuCommandBase
	{
		public cmdSampleDocument(MainWindow mainWindow, Control control) : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
		}

		public XMLSchemaWindow Window { get; private set; }

		public override void Execute(object parameter)
		{
            var g = new SampleDataGenerator();
            string d = g.Translate(ActiveDiagramView.Diagram as PSMDiagram);
            SampleDocumentWindow.Show(MainWindow.dockManager, d, ActiveDiagramView.Diagram as PSMDiagram, g.Log);
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.Diagram != null && ActiveDiagramView.Diagram is PSMDiagram;
		}
	}
}