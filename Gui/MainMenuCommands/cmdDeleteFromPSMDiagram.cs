using System;
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
	public class cmdDeleteFromPSMDiagram : MainMenuCommandBase
	{
		public cmdDeleteFromPSMDiagram(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		/// <summary>
		/// Removes selected elements from the active diagram.
		/// </summary>
		/// <seealso cref="IDeletable"/>
		/// <param name="parameter">passed boolean value can set the ForceDelete flag</param>
		public override void Execute(object parameter)
		{
			IEnumerable<IModelElementRepresentant> _deleted = ActiveDiagramView.SelectedRepresentants;
			List<Element> deleted = new List<Element>(_deleted.Count());
			foreach (IModelElementRepresentant deletable in _deleted)
			{
				deleted.Add(ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(deletable));	
			}
			DeleteFromPSMDiagramConsideringRepresentativesMacroCommand c = 
				(DeleteFromPSMDiagramConsideringRepresentativesMacroCommand)DeleteFromPSMDiagramConsideringRepresentativesMacroCommandFactory.Factory().Create(ActiveDiagramView.Controller);

			if (parameter is bool)
				c.ForceDelete = (bool)parameter;

			if (c.InitializeCommand(SelectionCallback, deleted))
			{
				c.Execute();
			}
		}

		private void SelectionCallback(IEnumerable<Element> t)
		{
			List<ISelectable> newSelection = new List<ISelectable>();
			foreach (Element element in t)
			{
				if (ActiveDiagramView.Diagram.IsElementPresent(element))
				{
					ISelectable s = ActiveDiagramView.ElementRepresentations[element] as ISelectable;
					if (s != null)
						newSelection.Add(s);
				}
			}
			ActiveDiagramView.SelectedItems.SetSelection(newSelection);
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.SelectedItems.OfType<IDeletable>().Count() > 0;
		}
	}
}