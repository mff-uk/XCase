using System.Collections.Generic;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using System.Linq;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds content container to the current diagram (made of selected components)
	/// </summary>
	public class cmdIntroduceContentContainer : MainMenuCommandBase
	{
		public cmdIntroduceContentContainer(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
			if (ActiveDiagramView != null && ActiveDiagramView.SelectedRepresentants.Count() > 0)
			{
				PSMSuperordinateComponent parent;
				IList<PSMSubordinateComponent> components;
				if (PSMDiagramHelper.AreComponentsOfCommonParent(ActiveDiagramView.SelectedRepresentants, out parent, out components))
				{
					NewPSMContentContainerCommand command =
						(NewPSMContentContainerCommand)NewPSMContentContainerCommandFactory.Factory().Create(ActiveDiagramView.Controller);
					command.Parent = parent;
					command.ContainedComponents.AddRange(components);
					command.Execute();
				}
			}
            else if (ActiveDiagramView != null && ActiveDiagramView.SelectedRepresentants.Count() == 0)
            {
                NewPSMContentContainerCommand command =
                    (NewPSMContentContainerCommand)NewPSMContentContainerCommandFactory.Factory().Create(ActiveDiagramView.Controller);
                command.Execute();
            }
		}

		/// <summary>
		/// Checks whether selected elements can be moved to a Content Container.
		/// </summary>
		/// <returns>
		/// true if this command can be executed; otherwise, false.      
		/// </returns>
		/// <param name="parameter">ignored</param>
		public override bool CanExecute(object parameter)
		{
            if (ActiveDiagramView != null && ActiveDiagramView.SelectedRepresentants.Count() > 0)
            {
                PSMSuperordinateComponent parent;
                IList<PSMSubordinateComponent> components;
                if (PSMDiagramHelper.AreComponentsOfCommonParent(ActiveDiagramView.SelectedRepresentants, out parent, out components))
                {
                    return true;
                }
            }
            else if (ActiveDiagramView != null && ActiveDiagramView.SelectedRepresentants.Count() == 0) return true;
			return false;
		}
	}
}
