using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using System.Linq;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Moves component out of content choice or content container
	/// </summary>
	public class cmdMoveComponentOutOfContainer : MainMenuCommandBase
	{
		public cmdMoveComponentOutOfContainer(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
		    MoveComponentOutOfContainerCommand command =
		        (MoveComponentOutOfContainerCommand)
		        MoveComponentOutOfContainerCommandFactory.Factory().Create(ActiveDiagramView.Controller);

            PSMSubordinateComponent sub = (PSMSubordinateComponent)ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(ActiveDiagramView.SelectedRepresentants.First());

            command.Container = sub.Parent;
            command.MovedComponents.Add(sub);
            command.Execute();
		}

		public override bool CanExecute(object parameter)
		{
            if (ActiveDiagramView == null || ActiveDiagramView.SelectedRepresentants.Count() != 1) 
                return false; 
		    
            Element e = ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(ActiveDiagramView.SelectedRepresentants.First());

            return e is PSMSubordinateComponent && (((PSMSubordinateComponent)e).Parent is PSMContentChoice || ((PSMSubordinateComponent)e).Parent is PSMContentContainer);
		}
	}
}