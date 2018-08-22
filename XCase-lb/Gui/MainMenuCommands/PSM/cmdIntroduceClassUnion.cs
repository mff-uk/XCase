using System;
using System.Linq;
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
	/// Adds class union to the current diagram (made of selected association)
	/// </summary>
	public class cmdIntroduceClassUnion : MainMenuCommandBase 
	{
		public cmdIntroduceClassUnion(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count > 0)
			{
				PSMSuperordinateComponent parent;
				IList<PSMSubordinateComponent> components;
				if (PSMDiagramHelper.AreComponentsOfCommonParent(ActiveDiagramView.SelectedRepresentants, out parent, out components))
				{
					if (components.All(component => component is PSMAssociation))
					{
						JoinAssociationsToClassUnionMacroCommand command = (JoinAssociationsToClassUnionMacroCommand)JoinAssociationsToClassUnionMacroCommandFactory.Factory().Create(ActiveDiagramView.Controller);
						command.Set(parent, components.Cast<PSMAssociation>().OrderBy(a => a.ComponentIndex()));
						command.Execute();
					}
				}
			}
		}

		public override bool CanExecute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count > 0)
			{
				PSMSuperordinateComponent parent;
				IList<PSMSubordinateComponent> components;
				if (PSMDiagramHelper.AreComponentsOfCommonParent(ActiveDiagramView.SelectedRepresentants, out parent, out components))
				{
					if (components.All(component => component is PSMAssociation))
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
