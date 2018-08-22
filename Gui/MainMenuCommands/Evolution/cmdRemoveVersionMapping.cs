using System.Windows.Controls;
using XCase.Gui.Windows;
using XCase.Model;
using XCase.Translation.XmlSchema;
using System.Linq;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Removes version mapping
	/// </summary>
	public class cmdRemoveVersionMapping: MainMenuCommandBase
	{
        public cmdRemoveVersionMapping(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		public override void Execute(object parameter)
		{
		    IVersionedElement element = GetElement();

		    CurrentProject.VersionManager.MakeIndependentOfOlderVersions(element);
            if (element is IHasPSMAttributes)
            {
                foreach (PSMAttribute a in ((IHasPSMAttributes)element).PSMAttributes)
                {
                    CurrentProject.VersionManager.MakeIndependentOfOlderVersions(a);
                }
            }

            if (element is IHasAttributes)
            {
                foreach (Property a in ((IHasAttributes)element).Attributes)
                {
                    CurrentProject.VersionManager.MakeIndependentOfOlderVersions(a);
                }
            }
		}

        private IVersionedElement GetElement()
        {
            return ActiveDiagramView.ElementRepresentations.GetElementRepresentedBy(ActiveDiagramView.SelectedRepresentants.First());
        }

        public override bool CanExecute(object parameter)
        {
            if (CurrentProject != null && CurrentProject.VersionManager != null && ActiveDiagramView != null && ActiveDiagramView.Diagram != null && ActiveDiagramView.SelectedRepresentants.Count() == 1)
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