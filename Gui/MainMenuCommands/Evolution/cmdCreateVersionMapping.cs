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
	public class cmdCreateVersionMapping: MainMenuCommandBase
	{
        public cmdCreateVersionMapping(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
            IVersionedElement selectedElement = GetElement();
		    Version currentVersion = selectedElement.Version;

			SelectItemsDialog selectVersion = new SelectItemsDialog();
            List<Version> _versions = new List<Version>(CurrentProject.VersionManager.Versions.Where(ver => ver.Number < currentVersion.Number));
		    selectVersion.UseRadioButtons = true;
            selectVersion.SetItems(_versions);
		    selectVersion.ShortMessage = "Select version";
		    selectVersion.LongMessage = "Select version to map to.";
		    selectVersion.Title = "Select version";

            if (selectVersion.ShowDialog() != true || selectVersion.selectedObjects.Count != 1)
                return;

		    Version v = (Version) selectVersion.selectedObjects[0];

		    Diagram diagram = (Diagram) ActiveDiagramView.Diagram.GetInVersion(v);
		    if (diagram == null)
		    {
		        XCaseYesNoBox.ShowOK("Diagram not present", "Diagram does not exist in this version. ");
		        return;
		    }

		    
		    List<Element> _candidates = new List<Element>();
		    foreach (var element in diagram.DiagramElements.Select(e => e.Key).Where
                (e => e.GetType() == selectedElement.GetType() && e.IsFirstVersion && e.GetInVersion(currentVersion) == null))
		    {
		        _candidates.Add(element);    
		    }

		    if (_candidates.Count == 0)
		    {
                XCaseYesNoBox.ShowOK("Not found", "No possible elements found for this item. ");
                return;
		    }

            SelectItemsDialog selectElement = new SelectItemsDialog();
            selectElement.UseRadioButtons = true;
            selectElement.SetItems(_candidates);
		    Element guess = _candidates.FirstOrDefault(c => c.ToString() == selectedElement.ToString());
		    selectElement.SelectItem(guess);
            selectElement.ShortMessage = "Select element";
            selectElement.LongMessage = "Select element to map to.";
            selectElement.Title = "Select element";

            if (selectElement.ShowDialog() != true || selectElement.selectedObjects.Count != 1)
                return;

            Element el = (Element)selectElement.selectedObjects[0];

		    CurrentProject.VersionManager.RegisterBranch(el, selectedElement,  currentVersion, true, el.Version);            
		}

        private IVersionedElement GetElement()
        {
            return ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(ActiveDiagramView.SelectedRepresentants.First());
        }

		public override bool CanExecute(object parameter)
		{
            if (CurrentProject != null && CurrentProject.VersionManager != null && ActiveDiagramView != null && ActiveDiagramView.Diagram != null && ActiveDiagramView.SelectedRepresentants.Count() == 1)
            {
                return GetElement().FirstVersion == GetElement();
            }
            else
            {
                return false; 
            }
		}
	}
}