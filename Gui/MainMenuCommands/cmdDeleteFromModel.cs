using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Interfaces;
using XCase.Gui.Dialogs;
using ElementDependencies = XCase.Controller.ElementDependencies;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Deletes element from model.
	/// </summary>
	public class cmdDeleteFromModel : MainMenuCommandBase
	{
		public cmdDeleteFromModel(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		/// <summary>
		/// Removes selected elements from the model
		/// </summary>
		/// <seealso cref="IDeletable"/>
		/// <param name="parameter"></param>
		public override void Execute(object parameter)
		{
			IEnumerable<IModelElementRepresentant> _deleted = ActiveDiagramView.SelectedRepresentants;
			List<Element> deleted = new List<Element>(_deleted.Count());
			foreach (IModelElementRepresentant deletable in _deleted)
			{
				deleted.Add(ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(deletable));	
			}
            DeleteFromModelMacroCommand c = (DeleteFromModelMacroCommand)DeleteFromModelMacroCommandFactory.Factory().Create(ModelController);
            c.Set(deleted, ActiveDiagramView.Controller);
            if (c.Commands.Count > 0)
            {
            	c.Execute();
				ActiveDiagramView.SelectedItems.SetSelection();
            }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.SelectedItems.OfType<IDeletable>().Count() > 0;
		}
	}
}