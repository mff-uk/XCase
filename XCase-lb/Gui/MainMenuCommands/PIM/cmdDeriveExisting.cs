using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Model;
using XCase.View.Controls;
using XCase.Controller.Commands;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Derives psm class into an existing diagram.
	/// </summary>
	public class cmdDeriveExisting: MainMenuCommandBase
	{
        public cmdDeriveExisting(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            PSMDiagram diagram = null;

            if (CurrentProject.PSMDiagrams.Count == 1) diagram = CurrentProject.PSMDiagrams[0];
            else if (CurrentProject.PSMDiagrams.Count > 1)
            {
                Dialogs.SelectPSMDiagramDialog d = new Dialogs.SelectPSMDiagramDialog();
                d.cmbDiagram.ItemsSource = CurrentProject.PSMDiagrams;
                d.cmbDiagram.SelectedIndex = 0;
                if (d.ShowDialog() == true) diagram = d.cmbDiagram.SelectedValue as PSMDiagram;
            }

            if (diagram == null) return;

            List<PIM_Class> List = new List<PIM_Class>();
            List.AddRange(ActiveDiagramView.SelectedItems.OfType<PIM_Class>());

            foreach (PIM_Class Class in List)
			{
                DerivePSMClassToDiagramCommand c = DerivePSMClassToDiagramCommandFactory.Factory().Create(CurrentProject.GetModelController()) as DerivePSMClassToDiagramCommand;
                c.Set(Class.ClassController.Class, diagram);
                if (c.CanExecute()) c.Execute();
			}
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null && ActiveDiagramView.SelectedItems.OfType<PIM_Class>().Count() > 0 && CurrentProject.PSMDiagrams.Count > 0;
		}
	}
}