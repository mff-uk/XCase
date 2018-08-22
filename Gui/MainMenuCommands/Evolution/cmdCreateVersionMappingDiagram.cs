using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using XCase.Controller.Dialogs;
using XCase.Gui.Windows;
using XCase.Model;
using XCase.Translation.XmlSchema;
using System.Linq;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Opens dialog for version mapping for diagram
	/// </summary>
	public class cmdCreateVersionMappingDiagram: MainMenuCommandBase
	{
        public cmdCreateVersionMappingDiagram(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
		    Diagram diagram = ActiveDiagramView.Diagram;
            Version currentVersion = CurrentProject.Version;

            SelectItemsDialog selectVersion = new SelectItemsDialog();
            List<Version> _versions = new List<Version>(CurrentProject.VersionManager.Versions.Where(ver => ver.Number < currentVersion.Number));
            selectVersion.UseRadioButtons = true;
            selectVersion.SetItems(_versions);
            selectVersion.ShortMessage = "Select version";
            selectVersion.LongMessage = "Select version to map to.";
            selectVersion.Title = "Select version";

            if (selectVersion.ShowDialog() != true || selectVersion.selectedObjects.Count != 1)
                return;

            Version v = (Version)selectVersion.selectedObjects[0];

		    List<Diagram> _candidates = CurrentProject.VersionManager.VersionedProjects[v].Diagrams.ToList();

            SelectItemsDialog selectDiagram = new SelectItemsDialog();
            selectDiagram.UseRadioButtons = true;
		    selectDiagram.ToStringAction = o => ((Diagram) o).Caption;
            selectDiagram.SetItems(_candidates);
            selectDiagram.ShortMessage = "Select diagram";
            selectDiagram.LongMessage = "Select diagram to map to.";
            selectDiagram.Title = "Select diagram";

            if (selectDiagram.ShowDialog() != true || selectDiagram.selectedObjects.Count != 1)
                return;

            Diagram diagramToMapTo = (Diagram)selectDiagram.selectedObjects[0];

		    CurrentProject.VersionManager.RegisterBranch(diagramToMapTo, diagram, currentVersion, true, v);            
		}

		public override bool CanExecute(object parameter)
		{
            if (CurrentProject != null && CurrentProject.VersionManager != null && ActiveDiagramView != null && 
                ActiveDiagramView.Diagram != null && 
                ActiveDiagramView.Diagram.FirstVersion == null)
            {
                return true; 
            }
            else
            {
                return false; 
            }
		}
	}
}