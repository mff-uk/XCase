using System.Linq;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
    /// <summary>
    /// Selects previous version of an element
    /// </summary>
    public class cmdLocatePreviousVersion : MainMenuCommandBase
    {
        public cmdLocatePreviousVersion(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
            MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
        }

        public override void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                ISelectable i = MainWindow.ActiveDiagram.SelectedItems[0];
                if (i is IModelElementRepresentant)
                {
                    Element e = MainWindow.ActiveDiagram.ElementRepresentations.GetElementRepresentedBy((IModelElementRepresentant)i);
                    Version v = MainWindow.ActiveDiagram.Diagram.Project.VersionManager.Versions[0];

                    if (e.ExistsInVersion(v) && MainWindow.DiagramTabManager.GetDiagramView((Diagram)MainWindow.ActiveDiagram.Diagram.GetInVersion(v)) != null)
                    {
                        MainWindow.DiagramTabManager.GetDiagramView((Diagram)MainWindow.ActiveDiagram.Diagram.GetInVersion(v)).SelectElement((Element)e.GetInVersion(v));
                    }
                }

            }
        }

        public override bool CanExecute(object parameter)
        {
            return MainWindow.ActiveDiagram != null && MainWindow.ActiveDiagram.SelectedItems.Count > 0 && MainWindow.CurrentProject.VersionManager != null;
            
        }
    }
}