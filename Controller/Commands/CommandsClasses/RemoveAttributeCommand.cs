using XCase.Model;
using System;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Removes attribute of an element implementing
	/// <see cref="IHasAttributes"/> (usually subtype of Class).
	/// </summary>
	public class RemoveAttributeCommand: ModelCommandBase
	{
		/// <summary>
		/// Creates new instance of <see cref="RemoveAttributeCommand" />. 
		/// </summary>
		/// <param name="controller">command controller</param>
		public RemoveAttributeCommand(ModelController controller)
			: base(controller)
		{
			Description = CommandDescription.REMOVE_ATTRIBUTE;
		}

		/// <summary>
		/// Deleted attribute
		/// </summary>
		[MandatoryArgument]
		public Property DeletedAttribute { get; set; }
		
		public override bool CanExecute()
		{
            if (DeletedAttribute == null) return false;
            if (DeletedAttribute.Class == null)
			{
				ErrorDescription = CommandError.CMDERR_REMOVING_DETACHED_ATTRIBUTE;
				return false; 
			}
			return true; 
		}

		internal override void CommandOperation()
		{
			AssociatedElements.Add(DeletedAttribute.Class);
			DeletedAttribute.RemoveMeFromModel();
		}

		internal override OperationResult UndoOperation()
		{
			DeletedAttribute.PutMeBackToModel();
			return OperationResult.OK;
		}
	}

	#region RemoveAttributeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="RemoveAttributeCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class RemoveAttributeCommandFactory : ModelCommandFactory<RemoveAttributeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private RemoveAttributeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of RemoveAttributeCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new RemoveAttributeCommand(modelController);
		}
	}

	#endregion
}
