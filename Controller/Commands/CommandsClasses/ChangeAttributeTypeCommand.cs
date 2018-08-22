using System;
using System.Diagnostics;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using System.Collections.ObjectModel;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Changes type of an attribute
	/// </summary>
	public class ChangeAttributeTypeCommand : ModelCommandBase
	{
		private DataType oldType;

		/// <summary>
		/// Modified attribute
		/// </summary>
		[MandatoryArgument]
		public Property Attribute { get; set; }

		/// <summary>
		/// New default value
		/// </summary>
		public ElementHolder<DataType> NewType { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="ChangeAttributeTypeCommand" />. 
		/// </summary>
		/// <param name="Controller">command controller</param>
		public ChangeAttributeTypeCommand(ModelController Controller)
			: base(Controller)
		{
			Description = CommandDescription.CHANGE_ATTRIBUTE_TYPE;
		}

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			oldType = Attribute.Type;
			Attribute.Type = NewType != null ? NewType.Element : null;
		}

		internal override OperationResult UndoOperation()
		{
			Attribute.Type = oldType;
			return OperationResult.OK;
		}
	}

	#region ChangeAttributeTypeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ChangeAttributeTypeCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class ChangeAttributeTypeCommandFactory : ModelCommandFactory<ChangeAttributeTypeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangeAttributeTypeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangeAttributeTypeCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new ChangeAttributeTypeCommand(modelController);
		}
	}

	#endregion
}