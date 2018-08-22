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

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Starts the proccess of XML Schema reverse engineering
	/// </summary>
    public class cmdXSDtoPSM : MainMenuCommandBase
    {
        public cmdXSDtoPSM(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public string Filename { get; set; } 

        public override void Execute(object parameter)
        {
            ReverseLog L = new ReverseLog();
            L.controller = MainWindow.ActiveDiagram.Controller;
            L.Filename = Filename;
            L.ShowDialog();
        }

        public override bool CanExecute(object parameter)
        {
            return CurrentProject != null && MainWindow.ActiveDiagram != null && MainWindow.ActiveDiagram.Diagram is PSMDiagram
                && (MainWindow.ActiveDiagram.Diagram as PSMDiagram).Roots.Count == 0;
        }
    }
}
