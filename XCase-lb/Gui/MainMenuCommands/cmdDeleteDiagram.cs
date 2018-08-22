using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;
using XCase.Controller.Commands;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Deletes elements from diagram.
	/// </summary>
	public class cmdDeleteDiagram : MainMenuCommandBase
	{
		public cmdDeleteDiagram(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            if (ActiveDiagramView.Diagram is PIMDiagram)
            {
                RemoveDiagramCommand c = RemoveDiagramCommandFactory.Factory().Create(CurrentProject.GetModelController()) as RemoveDiagramCommand;
                c.Set(CurrentProject, ActiveDiagramView.Diagram as PIMDiagram);
                c.Execute();
            }
            else if (ActiveDiagramView.Diagram is PSMDiagram)
            {
                RemovePSMDiagramMacroCommand c = RemovePSMDiagramMacroCommandFactory.Factory().Create(CurrentProject.GetModelController()) as RemovePSMDiagramMacroCommand;
                c.Set(CurrentProject, ActiveDiagramView.Diagram as PSMDiagram, ActiveDiagramView.Controller);
                if (c.Commands.Count > 0) c.Execute();
            }
            else throw new NotImplementedException();
		}

		public override bool CanExecute(object parameter)
		{
			return CurrentProject != null && ActiveDiagramView != null;
		}
	}
}