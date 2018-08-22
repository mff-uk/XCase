using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Model;
using XCase.View.Controls;
using XCase.Controller.Commands;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Derives PSM class to a new diagram.
	/// </summary>
	public class cmdDeriveNew: MainMenuCommandBase
	{
        public cmdDeriveNew(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            List<PIM_Class> List = new List<PIM_Class>();
            List.AddRange(ActiveDiagramView.SelectedItems.OfType<PIM_Class>());
            foreach (PIM_Class Class in List)
			{
                DerivePSMClassToNewDiagramCommand c = DerivePSMClassToNewDiagramCommandFactory.Factory().Create(CurrentProject.GetModelController()) as DerivePSMClassToNewDiagramCommand;
                c.Set(Class.ClassController.Class);
                if (c.CanExecute()) c.Execute();
			}
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.SelectedItems.OfType<PIM_Class>().Count() > 0;
		}
	}
}