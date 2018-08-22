using System.Collections.Generic;
using System.Linq;
using XCase.Controller.Dialogs;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Moves attributes from Attribute container to class.
	/// If the attribute container is left empty, user whether he wants to delete the container.
	/// </summary>
	public class MoveAttributesFromAttributeContainerBackToClassMacroCommand: MacroCommand<DiagramController>
	{
		public MoveAttributesFromAttributeContainerBackToClassMacroCommand(DiagramController controller) : base(controller)
		{
			Description = CommandDescription.PSM_MOVE_ATTRIBUTES_BACK_TO_CLASS;
		}

		public void InitializeCommand(PSMAttributeContainer attributeContainer, IEnumerable<PSMAttribute> attributes)
		{
			MoveAttributesFromAttributeContainerBackToClassCommand command = (MoveAttributesFromAttributeContainerBackToClassCommand)MoveAttributesFromAttributeContainerBackToClassCommandFactory.Factory().Create(Controller);
			command.AttributeContainer = attributeContainer;
			command.Attributes.AddRange(attributes);
			Commands.Add(command);

			if (attributeContainer.PSMAttributes.All(attribute => attributes.Contains(attribute)))
			{
				OkCancelDialog dialog = new OkCancelDialog();
				dialog.PrimaryContent = "Attribute container is empty.";
				dialog.Title = "Attribute container is empty";
				dialog.SecondaryContent = "Attribute container is now empty. Do you wish to remove the container now? ";
				dialog.OkButtonContent = "Delete";
				dialog.CancelButtonContent = "Leave";

				if (dialog.ShowDialog() == true)
				{
					DeleteFromPSMDiagramCommand leavecommand = (DeleteFromPSMDiagramCommand)DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller);
					leavecommand.DeletedElements = new[] { attributeContainer };
					Commands.Add(leavecommand);
				}
			}
		}
	}

	#region MoveAttributesFromAttributeContainerBackToClassMacroCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveAttributesFromAttributeContainerBackToClassMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveAttributesFromAttributeContainerBackToClassMacroCommandFactory : DiagramCommandFactory<MoveAttributesFromAttributeContainerBackToClassMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveAttributesFromAttributeContainerBackToClassMacroCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of MoveAttributesFromAttributeContainerBackToClassMacroCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveAttributesFromAttributeContainerBackToClassMacroCommand(diagramController);
		}
	}

	#endregion
}