using System;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds new operation to an element implementing
	/// <see cref="IHasOperations"/> (usually Class).
	/// </summary>
	public class NewOperationCommand: ModelCommandBase
    {
        /// <summary>
        /// Index from which is removed in Undo
        /// </summary>
        private int Index;
        
        /// <summary>
		/// Creates new instance of <see cref="NewOperationCommand" />. 
		/// </summary>
		/// <param name="controller">command controller</param>
		public NewOperationCommand(ModelController controller)
			: base(controller)
		{
			Description = CommandDescription.ADD_OPERATION;
		}

		[CommandResult]
		protected Operation createdOperation { get; set;}

		/// <summary>
		/// Element where new operation is created
		/// </summary>
		[MandatoryArgument]
		public IHasOperations Owner { get; set; }

		/// <summary>
		/// Name of the new operation 
		/// </summary>
		[MandatoryArgument]
    	public string Name { get; set; }

		/// <summary>
		/// Return type of the new operation 
		/// </summary>
		public DataType Type { get; set; }

		public override bool CanExecute()
        {
        	if (!NameSuggestor<Operation>.IsNameUnique(Owner.Operations, Name, operation => operation.Name))
        	{
        		ErrorDescription = String.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, Name);
        		return false; 
        	}
            return true;
        }

		internal override void CommandOperation()
        {
			if (Owner is Element)
				AssociatedElements.Add((Element)Owner);
			createdOperation = Owner.AddOperation();
			createdOperation.Name = Name;
        	createdOperation.Type = Type;
        }

		internal override OperationResult UndoOperation()
        {
            if (!Owner.Operations.Contains(createdOperation))
            {
                ErrorDescription = string.Format(CommandError.CMDERR_NOT_FOUND, createdOperation);
                return OperationResult.Failed;
            }
            else
            {
                Index = Owner.Operations.IndexOf(createdOperation);
                Owner.Operations.Remove(createdOperation);
                return OperationResult.OK;
            }
        }

        internal override void RedoOperation()
        {
            Owner.Operations.Insert(Index, createdOperation);
        }
    }

	#region NewOperationCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewOperationCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class NewOperationCommandFactory : ModelCommandFactory<NewOperationCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewOperationCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewOperationCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new NewOperationCommand(modelController);
		}
	}

	#endregion
}