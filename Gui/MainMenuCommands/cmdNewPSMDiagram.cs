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
	/// Creates new PSM diagram
	/// </summary>
	public class cmdNewPSMDiagram : MainMenuCommandBase
	{
		public cmdNewPSMDiagram(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			
		}

		public override void Execute(object parameter)
		{
            AddPSMDiagramCommand c = AddPSMDiagramCommandFactory.Factory().Create(CurrentProject.GetModelController()) as AddPSMDiagramCommand;
            c.Set(CurrentProject, null);
            c.Execute();
        }

		public override bool CanExecute(object parameter)
		{
			return true;
		}
	}
}