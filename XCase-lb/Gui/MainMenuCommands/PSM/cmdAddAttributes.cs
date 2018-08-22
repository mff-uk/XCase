using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.Gui.MainMenuCommands
{ 
	/// <summary>
	/// Adds attributes to PSM class.
	/// </summary>
	public class cmdAddAttributes : MainMenuCommandBase
	{
		public cmdAddAttributes(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			PSM_Class psmClass = (PSM_Class)ActiveDiagramView.SelectedItems.Single();

			(psmClass.Controller as PSM_ClassController).ShowClassDialog();
		}

		public override bool CanExecute(object parameter)
		{
			return (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count == 1 && ActiveDiagramView.SelectedItems.Single() is PSM_Class);
		}
	}
}