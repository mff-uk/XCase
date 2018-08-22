using System;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds an element to a diagram
	/// </summary>
	public class AddSimpleTypeCommand : ModelCommandBase
	{
		[MandatoryArgument]
		public string TypeName { get; set; }

		[MandatoryArgument]
		public Package Package { get; set; }

		[CommandResult]
		public ElementHolder<DataType> CreatedSimpleType { get; set; }

		public string XSDefinition { get; set; }

		public SimpleDataType Parent { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="AddSimpleTypeCommand">AddSimpleType</see>. 
		/// </summary>
		/// <param name="modelController">command controller</param>
		public AddSimpleTypeCommand(ModelController modelController)
			: base(modelController)
		{
			Description = CommandDescription.NEW_SIMPLE_TYPE;
		}

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			if (CreatedSimpleType == null)
				CreatedSimpleType = new ElementHolder<DataType>();
			SimpleDataType type = Package.AddSimpleDataType(Parent);
			type.Name = TypeName;

			type.DefaultXSDImplementation = XSDefinition;
			CreatedSimpleType.Element = type;
		}

		internal override OperationResult UndoOperation()
		{
			CreatedSimpleType.Element.RemoveMeFromModel();
			return OperationResult.OK;
		}

		internal override void RedoOperation()
		{
			CreatedSimpleType.Element.PutMeBackToModel();
		}
	}

	#region AddSimpleTypeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="AddSimpleTypeCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class AddSimpleTypeCommandFactory : ModelCommandFactory<AddSimpleTypeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private AddSimpleTypeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of AddSimpleTypeCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new AddSimpleTypeCommand(modelController);
		}
	}

	#endregion
}
