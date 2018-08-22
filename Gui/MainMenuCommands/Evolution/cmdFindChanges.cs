using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using XCase.Controller.Dialogs;
using XCase.Evolution;
using XCase.Gui.Windows;
using XCase.Model;
using XCase.Translation.XmlSchema;

namespace XCase.Gui.MainMenuCommands
{
	public class cmdFindChanges: MainMenuCommandBase
	{
        public cmdFindChanges(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            if (CanExecute(parameter))
            {
                ChangesDetector detector = new ChangesDetector();

                PSMDiagram diagramNewVersion = (PSMDiagram)ActiveDiagramView.Diagram;

                PSMDiagram diagramOldVersion = null;
                
                if (previousVersions.Count == 1)
                {
                    diagramOldVersion = (PSMDiagram) diagramNewVersion.GetInVersion(previousVersions.First());
                }
                else 
                {
                    SelectItemsDialog d = new SelectItemsDialog();
                    d.ShortMessage = "Select previous version of the diagram. ";
                    d.Title = "Select version";
                    d.UseRadioButtons = true;
                    d.SetItems(previousVersions);
                    d.SelectItem(previousVersions.FirstOrDefault(v => v.Number == previousVersions.Max(vm => vm.Number)));
                    if (d.ShowDialog() == true && d.selectedObjects.Count == 1)
                    {
                        diagramOldVersion = (PSMDiagram)d.selectedObjects[0];
                    }
                }

                if (diagramOldVersion != null)
                {
                    List<EvolutionChange> evolutionChanges = detector.Translate(diagramOldVersion, diagramNewVersion);
                    EvolutionChangesWindow.Show(evolutionChanges, MainWindow, diagramOldVersion, diagramNewVersion);
                }
            }
		}

        List<Version> previousVersions;

		public override bool CanExecute(object parameter)
		{
            if (ActiveDiagramView != null && ActiveDiagramView.Diagram != null && ActiveDiagramView.Diagram is PSMDiagram)
            {
                PSMDiagram psmDiagram = (PSMDiagram) ActiveDiagramView.Diagram;
                if (psmDiagram.VersionManager != null && psmDiagram.VersionManager.Versions.Count > 0)
                {
                    Version newVersion = psmDiagram.Version;                                  
                    
                    previousVersions = new List<Version>();

                    foreach (Version version in psmDiagram.VersionManager.Versions)
                    {
                        if (/*version.Number < newVersion.Number && */psmDiagram.GetInVersion(version) != null &&
                            version != psmDiagram.Version)
                        {
                            previousVersions.Add(version);
                        }
                    }
                    if (previousVersions.Count > 0)
                        return true;
                }
            }
		    return false; 
		}
	}
}