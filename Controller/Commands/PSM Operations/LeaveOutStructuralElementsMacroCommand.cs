using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
	public class LeaveOutStructuralElementsMacroCommand: MacroCommand<DiagramController>
	{
		public LeaveOutStructuralElementsMacroCommand(DiagramController controller) : base(controller)
		{
			Description = CommandDescription.PSM_LEAVE_OUT_STRUCTURAL_ELEMENTS;
		}

		public void InitializeCommand(IEnumerable<PSMSuperordinateComponent> superordinateComponents, IEnumerable<PSMClassUnion> unions, List<PSMAttributeContainer> attributeContainers)
		{
			foreach (PSMSuperordinateComponent component in superordinateComponents)
			{
				LeaveOutSuperordinateComponent c = (LeaveOutSuperordinateComponent)LeaveOutSuperordinateComponentFactory.Factory().Create(Controller);
				c.ComponentLeftOut = component;
				Commands.Add(c);
			}

			foreach (PSMClassUnion union in unions)
			{
				LeaveOutClassUnionCommand u = (LeaveOutClassUnionCommand)LeaveOutClassUnionCommandFactory.Factory().Create(Controller);
				u.UnionLeftOut = union;
				Commands.Add(u);
			}

			foreach (PSMAttributeContainer attributeContainer in attributeContainers)
			{
				LeaveOutAttributeContainerCommand command = (LeaveOutAttributeContainerCommand)LeaveOutAttributeContainerCommandFactory.Factory().Create(Controller);
				command.AttributeContainerLeftOut = attributeContainer;
				Commands.Add(command);
			}
		}
	}

	#region LeaveOutStructuralElementsMacroCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="LeaveOutStructuralElementsMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class LeaveOutStructuralElementsMacroCommandFactory : DiagramCommandFactory<LeaveOutStructuralElementsMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private LeaveOutStructuralElementsMacroCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of LeaveOutStructuralElementsMacroCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new LeaveOutStructuralElementsMacroCommand(diagramController);
		}
	}

	#endregion
}