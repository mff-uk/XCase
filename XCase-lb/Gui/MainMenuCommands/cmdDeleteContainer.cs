using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;
using XCase.View.Controls;
using XCase.View.Interfaces;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Issues command for removing selected PSM containers / non-semantic elements from the diagram
	/// </summary>
	public class cmdDeleteContainer : MainMenuCommandBase
	{
		public cmdDeleteContainer(MainWindow mainWindow, Control control)
			: base(mainWindow, control)
		{
			MainWindow.ActiveDiagramChanged += delegate { OnCanExecuteChanged(null); };
			MainWindow.DiagramSelectionChanged += delegate { OnCanExecuteChanged(null); };
		}

		/// <summary>
		/// Removes selected elements from the active diagram.
		/// </summary>
		/// <seealso cref="IDeletable"/>
		/// <param name="parameter"></param>
		public override void Execute(object parameter)
		{
			LeaveOutStructuralElementsMacroCommand command = (LeaveOutStructuralElementsMacroCommand)LeaveOutStructuralElementsMacroCommandFactory.Factory().Create(ActiveDiagramView.Controller);
			List<PSMSuperordinateComponent> superordinateComponents = new List<PSMSuperordinateComponent>();
			List<PSMClassUnion> unions = new List<PSMClassUnion>();
			List<PSMAttributeContainer> attributeContainers = new List<PSMAttributeContainer>();
			foreach (ISelectable item in ActiveDiagramView.SelectedItems)
			{
				IPSMSuperordinateComponentRepresentant super = item as IPSMSuperordinateComponentRepresentant;
				if (super != null)
					superordinateComponents.Add(super.ModelSuperordinateComponent);
				PSM_ClassUnion union = item as PSM_ClassUnion;
				if (union != null)
				{
					unions.Add(union.ClassUnion);
				}
				PSM_AttributeContainer attributeContainer = item as PSM_AttributeContainer;
				if (attributeContainer != null)
				{
					attributeContainers.Add(attributeContainer.AttributeContainer);
				}
			}
			command.InitializeCommand(superordinateComponents, unions, attributeContainers);
			command.Execute();
		}

		public override bool CanExecute(object parameter)
		{
			return ActiveDiagramView != null
				   && ActiveDiagramView.SelectedItems.Count > 0
				   && ActiveDiagramView.SelectedItems.All(item =>
					   !(item is PSM_Association)
					   && 
					   (
							(((item is IPSMSubordinateComponentRepresentant) && ((IPSMSubordinateComponentRepresentant)item).ModelSubordinateComponent.Parent != null))
							|| (item is PSM_ClassUnion)
							//|| (item is PSM_AttributeContainer)
					   )
					   );
			//!(item is PSM_Association) &&
			//(
			//     item is PSM_ClassUnion ||
			//     item is PSM_AttributeContainer ||
			//     item is PSM_ContentContainer ||
			//     item is PSM_ContentChoice
			//)
			//&&
			//(
			//     ((item is IPSMSubordinateComponentRepresentant) && ((IPSMSubordinateComponentRepresentant)item).ModelSubordinateComponent.Parent != null)) ||
			//     ((item is PSM_ClassUnion))
			//);
		}
	}
}