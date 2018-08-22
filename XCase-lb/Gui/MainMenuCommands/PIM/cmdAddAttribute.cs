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
	/// Adds attribute to selected class(s)
	/// </summary>
    public class cmdAddAttribute : MainMenuCommandBase
    {
        public cmdAddAttribute(MainWindow mainWindow, Control control)
            : base(mainWindow, control)
        {
            MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
        }

        public override void Execute(object parameter)
        {
            Element element;
            MacroCommand<DiagramController> command = MacroCommandFactory<DiagramController>.Factory().Create(ActiveDiagramView.Controller);
            command.Description = CommandDescription.ADD_ATTRIBUTE;
            foreach (ISelectable item in ActiveDiagramView.SelectedItems)
            {
                if (item is XCaseViewBase && (element = (item as XCaseViewBase).Controller.Element) is IHasAttributes)
                {
                    NewAttributeCommand c = NewAttributeCommandFactory.Factory().Create(ActiveDiagramView.Controller.ModelController) as NewAttributeCommand;
                    c.Owner = element as IHasAttributes;
                    c.Name = NameSuggestor<Property>.SuggestUniqueName((element as IHasAttributes).Attributes, "Attribute", property => property.Name);
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
