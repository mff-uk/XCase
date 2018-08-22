using System;
using System.Windows.Controls;
using XCase.View.Controls;
using XCase.Controller.Dialogs;
using System.Linq;
using XCase.Controller.Commands;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds childern of a PSM class (based on PIM associations of the class).
	/// </summary>
	public class cmdAddChildren : MainMenuCommandBase
	{
		public cmdAddChildren(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            PSM_Class psmClass = (PSM_Class)ActiveDiagramView.SelectedItems.Single();
            AddPSMChildrenMacroCommand c = AddPSMChildrenMacroCommandFactory.Factory().Create(ActiveDiagramView.Controller) as AddPSMChildrenMacroCommand;
            c.Set(psmClass.ClassController.Class);
            if (c.Commands.Count > 0) c.Execute();
		}

		public override bool CanExecute(object parameter)
		{
			return (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count == 1 && ActiveDiagramView.SelectedItems[0] is PSM_Class);
		}
	}
}