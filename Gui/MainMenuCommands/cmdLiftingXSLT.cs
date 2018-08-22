using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;
using XCase.Reverse;
using XCase.SemanticWS;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Generates a lifting XSLT
	/// </summary>
    public class cmdLiftingXSLT : MainMenuCommandBase
    {
        public cmdLiftingXSLT(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public string Filename { get; set; } 

        public override void Execute(object parameter)
        {
            LiftingXSLT L = new LiftingXSLT(MainWindow.ActiveDiagram.Controller);
            L.GenerateLiftingXSLT();
        }

        public override bool CanExecute(object parameter)
        {
            return CurrentProject != null && MainWindow.ActiveDiagram != null && MainWindow.ActiveDiagram.Diagram is PSMDiagram;
        }
    }
}
