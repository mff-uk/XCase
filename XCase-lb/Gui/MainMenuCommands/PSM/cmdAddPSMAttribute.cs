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

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Adds attribute to selected PSM class(s)
	/// </summary>
    public class cmdAddPSMAttribute : MainMenuCommandBase
    {
        public cmdAddPSMAttribute(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public override void Execute(object parameter)
        {
            Element element;
            MacroCommand<DiagramController> command = MacroCommandFactory<DiagramController>.Factory().Create(ActiveDiagramView.Controller);
            command.Description = CommandDescription.ADD_PSM_ATTRIBUTE;
            foreach (ISelectable item in ActiveDiagramView.SelectedItems)
            {
                if (item is PSM_Class && (element = (item as PSM_Class).Controller.Element) is PSMClass)
                {
                    AddPSMClassAttributeCommand c = AddPSMClassAttributeCommandFactory.Factory().Create(ActiveDiagramView.Controller) as AddPSMClassAttributeCommand;
                    c.PSMClass = (PSMClass) element;
                    c.Name = NameSuggestor<PSMAttribute>.SuggestUniqueName(((PSMClass)element).PSMAttributes, "FreeAttribute", property => property.AliasOrName);
                    command.Commands.Add(c);
                }
            }
            if (command.Commands.Count > 0) command.Execute();
        }

        private bool Check(ISelectable item)
        {
            return item is PSM_Class && (item as PSM_Class).Controller.Element is IHasPSMAttributes;
        }
        
        public override bool CanExecute(object parameter)
        {
            return (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count > 0 && ActiveDiagramView.SelectedItems.All(Check));
        }
    }
}
