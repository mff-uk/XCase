using System.Linq;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Gui.Dialogs;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.Gui.MainMenuCommands
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="cmdRemovePSMDiagramReference"/>
    public class cmdAddPSMDiagramReference : MainMenuCommandBase
    {
        public cmdAddPSMDiagramReference(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public override void Execute(object parameter)
        {
            Dialogs.SelectPSMDiagramDialog dialog = new SelectPSMDiagramDialog(CurrentProject.PSMDiagrams.Where(d => d != ActiveDiagramView.Diagram));
            dialog.textBlock1.Content = "Select referenced PSM diagram";
            if (dialog.ShowDialog() == true)
            {
                AddPSMDiagramReferenceCommand command = (AddPSMDiagramReferenceCommand)AddPSMDiagramReferenceCommandFactory.Factory().Create(ActiveDiagramView.Controller);
                command.ReferencedDiagram = dialog.SelectedDiagram;
                command.Execute();
            }
        }

        public override bool CanExecute(object parameter)
        {
            return (ActiveDiagramView != null
                    && ActiveDiagramView.Diagram is PSMDiagram);
        }
    }
}