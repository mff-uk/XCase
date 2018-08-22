using System;
using System.Diagnostics;
using XCase.Model;
using System.Collections.ObjectModel;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Sets attribute default value
	/// </summary>
	public class ChangeAttributeDefaultValueCommand : ModelCommandBase
	{
		private string oldDefault;

		/// <summary>
		/// Modified attribute
		/// </summary>
		[MandatoryArgument]
		public Property Attribute { get; set; }

		/// <summary>
		/// New default value
		/// </summary>
		public string NewDefault { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="ChangeAttributeDefaultValueCommand" />. 
		/// </summary>
		/// <param name="Controller">command controller</param>
		public ChangeAttributeDefaultValueCommand(ModelController Controller)
			: base(Controller)
		{
			Description = CommandDescription.CHANGE_ATTRIBUTE_DEFAULT;
		}

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			AssociatedElements.Add(Attribute.Class);
			oldDefault = Attribute.Default;
			Attribute.Default = !String.IsNullOrEmpty(NewDefault) ? NewDefault : null;
		}

		internal override OperationResult UndoOperation()
		{
			Attribute.Default = oldDefault;
			return OperationResult.OK;
		}
	}

	/// <summary>
	/// Factory that creates instances of <see cref="ChangeAttributeDefaultValueCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class ChangeAttributeDefaultValueCommandFactory : ModelCommandFactory<ChangeAttributeDefaultValueCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangeAttributeDefaultValueCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangeAttributeDefaultValueCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new ChangeAttributeDefaultValueCommand(modelController);
		}
	}
}