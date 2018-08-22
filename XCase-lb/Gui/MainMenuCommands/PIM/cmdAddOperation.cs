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
	/// Adds operation to a class
	/// </summary>
    public class cmdAddOperation : MainMenuCommandBase
    {
        public cmdAddOperation(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public override void Execute(object parameter)
        {
            Element element;
            MacroCommand<DiagramController> command = MacroCommandFactory<DiagramController>.Factory().Create(ActiveDiagramView.Controller);
            command.Description = CommandDescription.ADD_OPERATION;
            foreach (ISelectable item in ActiveDiagramView.SelectedItems)
            {
                if (item is XCaseViewBase && (element = (item as XCaseViewBase).Controller.Element) is IHasOperations)
                {
                    NewOperationCommand c = NewOperationCommandFactory.Factory().Create(ActiveDiagramView.Controller.ModelController) as NewOperationCommand;
                    c.Owner = element as IHasOperations;
                    c.Name = NameSuggestor<Operation>.SuggestUniqueName((element as IHasOperations).Operations, "Operation", property => property.Name);
                    command.Commands.Add(c);
                }
            }
            if (command.Commands.Count > 0) command.Execute();
        }

        private bool Check(ISelectable item)
        {
            return item is XCaseViewBase && (item as XCaseViewBase).Controller.Element is IHasAttributes;
        }

        public override bool CanExecute(object parameter)
        {
            return (ActiveDiagramView != null && ActiveDiagramView.SelectedItems.Count > 0 && ActiveDiagramView.SelectedItems.All(Check));
        }
    }
}
