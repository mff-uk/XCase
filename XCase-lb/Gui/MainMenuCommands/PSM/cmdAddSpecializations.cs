using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds PSM specializations of the selected class (if PIM specializations exist)
	/// </summary>
	public class cmdAddSpecializations: MainMenuCommandBase
	{
		public cmdAddSpecializations(MainWindow mainWindow, Control control) : base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			PSM_Class psmClass = (PSM_Class)ActiveDiagramView.SelectedItems.Single();
			IncludeElementsDialog dialog = new IncludeElementsDialog();
			dialog.Title = "Select specializations";
			dialog.PrimaryContent = "Select specializations";
			dialog.SecondaryContent = "Select specializations of the class you wish to include in the diagram: ";
			dialog.NoElementsContent = "There are no specializations or all the specializations are already present in the diagram. ";
			IEnumerable<Element> alreadyPresent = psmClass.PSMClass.Specifications.Select(generalization => ((PSMClass)generalization.Specific).RepresentedClass).Cast<Element>();
			dialog.Items = psmClass.PSMClass.RepresentedClass.Specifications.Select(generalization => generalization.Specific).Cast<Element>().Except(alreadyPresent);
			if (dialog.ShowDialog() == true)
			{
				if (dialog.SelectedElements.Count > 0)
				{
					ActiveDiagramView.Controller.BeginMacro();
					foreach (Class specialization in dialog.SelectedElements)
					{
						psmClass.ClassController.AddClassSpecialization((PIMClass)specialization);
					}
					ActiveDiagramView.Controller.CommitMacro();
				}
			}
		}

		public override bool CanExecute(object parameter)
		{
			return (
				ActiveDiagramView != null 
				&& ActiveDiagramView.SelectedItems.Count == 1 
				&& ActiveDiagramView.SelectedItems[0] is PSM_Class
				&& ((PSM_Class)ActiveDiagramView.SelectedItems[0]).PSMClass.RepresentedClass.Specifications.Count > 0
				);
		}
	}
}