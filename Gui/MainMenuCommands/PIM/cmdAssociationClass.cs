using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Geometries;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds association class connecting currently selected classes.
	/// </summary>
	internal class cmdAssociationClass : MainMenuCommandBase
	{
		public cmdAssociationClass(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            IEnumerable<PIM_Class> selectedClasses = ActiveDiagramView.SelectedItems.OfType<PIM_Class>();
			Class[] classes = new Class[selectedClasses.Count()];
			int i = 0;
            foreach (PIM_Class xCaseClass in selectedClasses)
			{
				classes[i] = (Class)xCaseClass.ModelElement;
				i++;
			}

			Rect r = RectExtensions.GetEncompassingRectangle(selectedClasses.OfType<IConnectable>());
			if (selectedClasses.Count() > 2)
			{
				ActiveDiagramView.Controller.NewAssociationClass(null, classes); //r.GetCenter().X + 30, r.GetCenter().Y );
			}
			else
			{
				ActiveDiagramView.Controller.NewAssociationClass(null, classes); //r.GetCenter().X, r.GetCenter().Y + 20);
			}

			
		}

		public override bool CanExecute(object parameter)
		{
			//return true; 
			return ActiveDiagramView != null && ActiveDiagramView.SelectedItems.OfType<XCaseViewBase>().Count() > 0;
		}
	}
}