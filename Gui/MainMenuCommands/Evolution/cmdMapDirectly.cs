using System.Linq;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
    /// <summary>
    /// Maps two selected items (without dialogs)
    /// </summary>
    public class cmdMapDirectly : MainMenuCommandBase
    {
        public cmdMapDirectly(MainWindow mainWindow, Control control)
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
                    Version v;

                    Version newerVersion = MainWindow.ActiveDiagram.Diagram.Project.VersionManager.Versions[1];
                    Version olderVersion = MainWindow.ActiveDiagram.Diagram.Project.VersionManager.Versions[0];
                    
                    if (e.Version == newerVersion)
                    {
                        v = olderVersion;
                    }
                    else
                        v = newerVersion;

                    XCaseCanvas xCaseCanvas = MainWindow.DiagramTabManager.GetDiagramView((Diagram)MainWindow.ActiveDiagram.Diagram.GetInVersion(v));
                    if (xCaseCanvas.SelectedItems.Count == 1)
                    {
                        ISelectable i2 = xCaseCanvas.SelectedItems[0];
                        if (i2 is IModelElementRepresentant)
                        {
                            Element e2 = xCaseCanvas.ElementRepresentations.GetElementRepresentedBy((IModelElementRepresentant)i2);

                            Element source = e.Version == olderVersion ? e : e2;
                            Element branch = e.Version == newerVersion ? e : e2;

                            MainWindow.CurrentProject.VersionManager.RegisterBranch(source, branch, newerVersion, true, olderVersion);
                        }
                    }
                }
            }
        }

        public override bool CanExecute(object parameter)
        {
            if (MainWindow.ActiveDiagram != null && MainWindow.ActiveDiagram.SelectedItems.Count == 1 && MainWindow.CurrentProject.VersionManager != null)
            {
                ISelectable i = MainWindow.ActiveDiagram.SelectedItems[0];
                if (i is IModelElementRepresentant)
                {
                    Element e =
                        MainWindow.ActiveDiagram.ElementRepresentations.GetElementRepresentedBy(
                            (IModelElementRepresentant) i);
                    Version v;

                    Version newerVersion = MainWindow.ActiveDiagram.Diagram.Project.VersionManager.Versions[1];
                    Version olderVersion = MainWindow.ActiveDiagram.Diagram.Project.VersionManager.Versions[0];

                    if (e.Version == newerVersion)
                    {
                        v = olderVersion;
                    }
                    else
                        v = newerVersion;

                    XCaseCanvas xCaseCanvas =
                        MainWindow.DiagramTabManager.GetDiagramView(
                            (Diagram) MainWindow.ActiveDiagram.Diagram.GetInVersion(v));
                    if (xCaseCanvas.SelectedItems.Count == 1)
                    {
                        ISelectable i2 = xCaseCanvas.SelectedItems[0];
                        if (i2 is IModelElementRepresentant)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}