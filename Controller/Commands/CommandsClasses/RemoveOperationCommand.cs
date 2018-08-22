using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Removes operation from an element implementing <see cref="IHasOperations"/>
	/// </summary>
	public class RemoveOperationCommand: ModelCommandBase
	{
		/// <summary>
		/// Creates new instance of <see cref="RemoveOperationCommand" /> (usually Class).
		/// </summary>
		/// <param name="controller">command controller</param>
		public RemoveOperationCommand(ModelController controller)
			: base(controller)
		{
            Description = CommandDescription.REMOVE_OPERATION;
		}

		/// <summary>
		/// Deleted operation
		/// </summary>
		[MandatoryArgument]
		public Operation DeletedOperation { get; set; }

		public override bool CanExecute()
		{
			if (DeletedOperation.Class == null)
			{
				ErrorDescription = CommandError.CMDERR_REMOVING_DETACHED_OPERATION;
				return false; 
			}
			return true; 
		}

		internal override void CommandOperation()
		{
			AssociatedElements.Add(DeletedOperation.Class);
			DeletedOperation.RemoveMeFromModel();
		}

		internal override OperationResult UndoOperation()
		{
			DeletedOperation.PutMeBackToModel();
			return OperationResult.OK;
		}
	}

	#region RemoveOperationCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="RemoveOperationCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class RemoveOperationCommandFactory : ModelCommandFactory<RemoveOperationCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private RemoveOperationCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of RemoveOperationCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new RemoveOperationCommand(modelController);
		}
	}

	#endregion
}