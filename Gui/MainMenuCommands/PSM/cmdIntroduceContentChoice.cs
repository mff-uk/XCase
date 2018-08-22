using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds content choice to the current diagram (made of selected components)
	/// </summary>
	public class cmdIntroduceContentChoice : MainMenuCommandBase
	{
		public cmdIntroduceContentChoice(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count > 1)
			{
				PSMSuperordinateComponent parent;
				IList<PSMSubordinateComponent> components;
				if (PSMDiagramHelper.AreComponentsOfCommonParent(ActiveDiagramView.SelectedRepresentants, out parent, out components))
				{
					NewPSMContentChoiceCommand command = (NewPSMContentChoiceCommand)NewPSMContentChoiceCommandFactory.Factory().Create(ActiveDiagramView.Controller);
					command.Parent = parent;
					command.ContainedComponents.AddRange(components);
					command.Execute();
				}
			}
		}

		/// <summary>
		/// Checks whether selected elements can be moved to a Content Choice.
		/// </summary>
		/// <returns>
		/// true if this command can be executed; otherwise, false.      
		/// </returns>
		/// <param name="parameter">ignored</param>
		public override bool CanExecute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count > 1)
			{
				PSMSuperordinateComponent parent;
				IList<PSMSubordinateComponent> components;
				if (PSMDiagramHelper.AreComponentsOfCommonParent(ActiveDiagramView.SelectedRepresentants, out parent, out components))
				{
					return true;
				}
			}
			return false;
		}
	}
}
