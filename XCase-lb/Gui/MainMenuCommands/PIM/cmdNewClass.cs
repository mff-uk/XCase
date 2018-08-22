using System.Windows.Controls;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;
using System.Linq;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Command that initializes dragging of a new class into the active diagram.
	/// </summary>
	/// <see cref="XCaseCanvas.DraggingElementState"/>
	public class cmdNewClass: cmdInsertElement
	{
		public cmdNewClass(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			
		}

		private string className;

		protected override DragButtonData PrepareButtonData()
		{
			className = NameSuggestor<PIMClass>.SuggestUniqueName(ActiveDiagramView.Controller.ModelController.Model.Classes, "Class", modelClass => modelClass.Name);
			return new DragButtonData(NewModelClassToDiagramCommandFactory.Factory(), new PIM_Class(ActiveDiagramView)
			                                                                          	{
			                                                                          		ElementName = className
			                                                                          	});
		}

		protected override void ElementInserted(System.Collections.Generic.IList<Element> elements)
		{
			Element element = elements.FirstOrDefault();
			if (element != null && ActiveDiagramView.ElementRepresentations.IsElementPresent(element))
			{
				if (element is PIMClass && ActiveDiagramView.ElementRepresentations[element] is PIM_Class)
				{
					//((PIM_Class)ActiveDiagramView.ElementRepresentations[element]).ClassController.ShowClassDialog();
				}
			}
		}

		protected override void InitializeCommand(NewPositionableElementMacroCommand command)
		{
			base.InitializeCommand(command);
			(command as NewModelClassToDiagramCommand).ClassName = className;
		}
	}
}
