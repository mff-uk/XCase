using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Issues command for removing selected elements from the diagram
	/// </summary>
	public class cmdDeleteFromDiagram : MainMenuCommandBase
	{
		public cmdDeleteFromDiagram(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		/// <summary>
		/// Removes selected elements from the active diagram.
		/// </summary>
		/// <seealso cref="IDeletable"/>
		/// <param name="parameter"></param>
		public override void Execute(object parameter)
		{
            DeleteFromDiagramMacroCommand c =
                (DeleteFromDiagramMacroCommand)DeleteFromDiagramMacroCommandFactory.Factory().Create(ActiveDiagramView.Controller);
			IEnumerable<IModelElementRepresentant> _deleted = ActiveDiagramView.SelectedRepresentants;
			List<Element> deleted = new List<Element>(_deleted.Count());
			foreach (IModelElementRepresentant deletable in _deleted)
			{
				deleted.Add(ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(deletable));	
			}
			if (c.InitializeCommand(deleted))
			{
				c.Execute();
				ActiveDiagramView.SelectedItems.SetSelection();
			}
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.SelectedItems.OfType<IDeletable>().Count() > 0;
		}
	}
}