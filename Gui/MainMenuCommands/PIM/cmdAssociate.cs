using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds association connecting currently selected classes.
	/// </summary>
	public class cmdAssociate: MainMenuCommandBase
	{
		public cmdAssociate(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            IEnumerable<PIM_Class> selectedClasses = ActiveDiagramView.SelectedItems.OfType<PIM_Class>();
			Class[] classes = new Class[selectedClasses.Count()];
			int i = 0;
            foreach (PIM_Class xCaseClass in selectedClasses)
			{
				classes[i] = (Class)xCaseClass.ModelElement;
				i++;
			}

			ActiveDiagramView.Controller.NewAssociation(null, classes);
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.SelectedItems.OfType<XCaseViewBase>().Count() > 0;
		}
	}
}