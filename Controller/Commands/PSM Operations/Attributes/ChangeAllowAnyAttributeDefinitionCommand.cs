using System;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Sets value of <see cref="XCase.Model.PSMClass.AllowAnyAttribute"/>.
	/// </summary>
	public class ChangeAllowAnyAttributeDefinitionCommand: DiagramCommandBase
	{
		[MandatoryArgument] 
		public bool? AllowAnyAttribute { get; set; }

		[MandatoryArgument]
		public PSMClass PSMClass { get; set; }

		/// <summary>
		/// Creates new instance of DiagramCommandBase.
		/// </summary>
		/// <param name="controller">diagram controller that will manage command execution</param>
		public ChangeAllowAnyAttributeDefinitionCommand(DiagramController controller) : base(controller)
		{
			Description = CommandDescription.CHANGE_ANY_ATTRIBUTE;
		}

		/// <summary>
		/// Returns true if command can be executed.
		/// </summary>
		/// <returns>True if command can be executed</returns>
		public override bool CanExecute()
		{
			return true;
		}

		private bool oldValue; 

		/// <summary>
		/// Executive function of a command
		/// </summary>
		/// <seealso cref="UndoOperation"/>
		internal override void CommandOperation()
		{
			oldValue = PSMClass.AllowAnyAttribute;
			PSMClass.AllowAnyAttribute = AllowAnyAttribute.Value;
			AssociatedElements.Add(PSMClass);
		}

		/// <summary>
		/// Undo executive function of a command. Should revert the <see cref="CommandOperation"/> executive 
		/// function and return the state to the state before CommandOperation was execute.
		/// <returns>returns <see cref="CommandBase.OperationResult.OK"/> if operation succeeded, <see cref="CommandBase.OperationResult.Failed"/> otherwise</returns>
		/// </summary>
		/// <remarks>
		/// <para>If  <see cref="CommandBase.OperationResult.Failed"/> is returned, whole undo stack is invalidated</para>
		/// </remarks>
		internal override OperationResult UndoOperation()
		{
			PSMClass.AllowAnyAttribute = oldValue;
			return OperationResult.OK;
		}
	}

	#region ChangeAllowAnyAttributeDefinitionCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ChangeAllowAnyAttributeDefinitionCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ChangeAllowAnyAttributeDefinitionCommandFactory : DiagramCommandFactory<ChangeAllowAnyAttributeDefinitionCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangeAllowAnyAttributeDefinitionCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangeAllowAnyAttributeDefinitionCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ChangeAllowAnyAttributeDefinitionCommand(diagramController);
		}
	}

	#endregion
}