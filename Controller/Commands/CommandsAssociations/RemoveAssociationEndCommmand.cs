using System;
using XCase.Model;

namespace XCase.Controller.Commands.CommandsAssociations
{
	public class RemoveAssociationEndCommmand: ModelCommandBase
	{

		/// <summary>
		/// Association end removed from the association 
		/// </summary>
		[CommandResult]
		public AssociationEnd AssociationEnd { get; set; }


		/// <summary>
		/// Creates new instance of ModelCommandBase.
		/// </summary>
		/// <param name="controller">model controller that will manage command execution</param>
		public RemoveAssociationEndCommmand(ModelController controller) : base(controller)
		{
			Description = CommandDescription.REMOVE_ASSOCIATION_END;
		}

		/// <summary>
		/// Returns true if command can be executed.
		/// </summary>
		/// <returns>True if command can be executed</returns>
		public override bool CanExecute()
		{
			if (AssociationEnd.Association.Ends.Count == 2)
			{
				ErrorDescription = CommandError.CANNOT_REMOVE_END_FROM_BINARY_ASSOCIATION;
				return false; 
			}
			return true;
		}

		/// <summary>
		/// Executive function of a command
		/// </summary>
		/// <seealso cref="UndoOperation"/>
		internal override void CommandOperation()
		{
			AssociationEnd.Association.RemoveEnd(AssociationEnd);
			AssociatedElements.Add(AssociationEnd.Association);
            
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
			AssociationEnd.Association.Ends.Add(AssociationEnd);
			return OperationResult.OK;
		}
	}

	#region RemoveAssociationEndCommmandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="RemoveAssociationEndCommmand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class RemoveAssociationEndCommmandFactory : ModelCommandFactory<RemoveAssociationEndCommmandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private RemoveAssociationEndCommmandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of RemoveAssociationEndCommmand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new RemoveAssociationEndCommmand(modelController);
		}
	}

	#endregion
}