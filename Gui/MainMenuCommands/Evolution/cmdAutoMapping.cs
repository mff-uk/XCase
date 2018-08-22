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
	/// Opens dialog for version mapping
	/// </summary>
	public class cmdAutoMapping: MainMenuCommandBase
	{
        public cmdAutoMapping(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

        private List<Version> FindLowerVersions(Project project)
        {
            return new List<Version>(CurrentProject.VersionManager.Versions.Where(
                ver => ver.Number < project.Version.Number));
        }

		public override void Execute(object parameter)
		{
            Version currentVersion = CurrentProject.Version;

			SelectItemsDialog selectVersion = new SelectItemsDialog();
            List<Version> _versions = FindLowerVersions(CurrentProject);
		    if (_versions.Count == 0)
		    {
                XCaseYesNoBox.ShowOK("No previous version", "No previous version of the diagram");
                return;
		    }
		    Version v;
		    if (_versions.Count > 1)
            {
                selectVersion.UseRadioButtons = true;
                selectVersion.SetItems(_versions);
                selectVersion.ShortMessage = "Select version";
                selectVersion.LongMessage = "Select version to map to.";
                selectVersion.Title = "Select version";

                if (selectVersion.ShowDialog() != true || selectVersion.selectedObjects.Count != 1)
                    return;
                v = (Version)selectVersion.selectedObjects[0];
            }
		    else
		    {
		        v = _versions.First();
		    }

		    Diagram diagramOldVersion = (Diagram) ActiveDiagramView.Diagram.GetInVersion(v);
		    if (diagramOldVersion == null)
		    {
		        XCaseYesNoBox.ShowOK("Diagram not present", "Diagram does not exist in this version. ");
		        return;
		    }

            mappingDialog = new MappingDialog();
		    mappingDialog.DiagramNewVersion = ActiveDiagramView.Diagram;
		    mappingDialog.DiagramOldVersion = diagramOldVersion;
            mappingDialog.DiagramView = ActiveDiagramView;
            mappingDialog.DiagramViewOldVersion = MainWindow.DiagramTabManager.GetDiagramView(diagramOldVersion);
            mappingDialog.Closed += new System.EventHandler(mappingDialog_Closed);
            mappingDialog.Show();
		}

        MappingDialog mappingDialog;

        void mappingDialog_Closed(object sender, System.EventArgs e)
        {
            if (!mappingDialog.DR)
                return;

            foreach (MappingDialog.MappingGridItem item in mappingDialog.ItemsList)
            {
                if (item.OriginalMapping != item.OldVersionConstruct)
                {
                    CurrentProject.VersionManager.RegisterBranch(item.OldVersionConstruct, item.NewVersionConstruct, mappingDialog.DiagramNewVersion.Version, true, mappingDialog.DiagramOldVersion.Version);
                }
            }
        }

		public override bool CanExecute(object parameter)
		{
            if (CurrentProject != null && 
                CurrentProject.VersionManager != null && 
                ActiveDiagramView != null && 
                ActiveDiagramView.Diagram != null && 
                ActiveDiagramView.Diagram.FirstVersion != null &&
                ActiveDiagramView.Diagram.IsFirstVersion == false)
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