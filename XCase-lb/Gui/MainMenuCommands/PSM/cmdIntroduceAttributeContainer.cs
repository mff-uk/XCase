using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds attribute container to the current diagram (made of selected components)
	/// </summary>
	public class cmdIntroduceAttributeContainer: MainMenuCommandBase
	{
		public cmdIntroduceAttributeContainer(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			PSM_Class psmClass = (PSM_Class)ActiveDiagramView.SelectedItems.Single();

			SelectAttributesDialog dialog = new SelectAttributesDialog
            	{
            		ShowAliasTextBox = false,
            		PSMElementsOnly = true,
            		MessageText = "Select attributes you wish to move to an attribute container:",
            		Element = psmClass.ClassController.Class
            	};
			if (dialog.ShowDialog() == true)
			{
				NewPSMAttributeContainerCommand c = (NewPSMAttributeContainerCommand)NewPSMAttributeContainerCommandFactory.Factory().Create(ActiveDiagramView.Controller);
				c.PSMClass = (PSMClass)(psmClass.Controller.Element);
				c.PSMAttributes.AddRange(dialog.SelectedPSMAttributes);
				c.Execute();
			}
		}

		public override bool CanExecute(object parameter)
		{
			return (ActiveDiagramView != null 
				&& ActiveDiagramView.SelectedItems.Count == 1 
				&& ActiveDiagramView.SelectedItems.Single() is PSM_Class
				&& ((PSM_Class)ActiveDiagramView.SelectedItems.Single()).PSMClass.Attributes.Count > 0
				);
		}
	}
}
