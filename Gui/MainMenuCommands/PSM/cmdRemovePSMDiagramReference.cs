using System.Linq;
using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;

namespace XCase.Gui.MainMenuCommands
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="cmdAddPSMDiagramReference"/>
    public class cmdRemovePSMDiagramReference : MainMenuCommandBase
    {
        public cmdRemovePSMDiagramReference(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public override void Execute(object parameter)
        {

        }

        public override bool CanExecute(object parameter)
        {
            return (ActiveDiagramView != null
                    && ActiveDiagramView.SelectedItems.Count == 1
                    && ActiveDiagramView.SelectedItems[0] is PSM_Class);
        }
    }
}