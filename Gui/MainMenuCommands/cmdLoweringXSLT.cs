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
	/// Generates a lowering XSLT
	/// </summary>
    public class cmdLoweringXSLT : MainMenuCommandBase
    {
        public cmdLoweringXSLT(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public string Filename { get; set; } 

        public override void Execute(object parameter)
        {
            LoweringXSLT L = new LoweringXSLT(MainWindow.ActiveDiagram.Controller);
            L.GenerateLoweringXSLT();
        }

        public override bool CanExecute(object parameter)
        {
            return CurrentProject != null && MainWindow.ActiveDiagram != null && MainWindow.ActiveDiagram.Diagram is PSMDiagram;
        }
    }
}
