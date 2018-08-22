using System;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds an element to a diagram
	/// </summary>
	public class AlterSimpleTypeCommand : ModelCommandBase
	{
		[MandatoryArgument]
		public SimpleDataType SimpleDataType { get; set; }

		public string Name { get; set; }

		//public Package Package { get; set; }

		//public SimpleDataType Parent { get; set; }

		public string XSDImplementation { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="AlterSimpleTypeCommand">AlterSimpleTypeCommand</see>. 
		/// </summary>
		/// <param name="modelController">command controller</param>
		public AlterSimpleTypeCommand(ModelController modelController)
			: base(modelController)
		{
			Description = CommandDescription.PSM_COMPONENT_REORDER;
		}

		public override bool CanExecute()
		{
			if (Name != SimpleDataType.Name)
				return NameSuggestor<DataType>.IsNameUnique(SimpleDataType.Package.OwnedTypes, Name, item => item.Name);
			else
				return true; 
		}

		private string oldName;
		//private Package oldPackage;
		//private SimpleDataType oldParent;
		private string oldXSD;

		internal override void CommandOperation()
		{
			oldName = SimpleDataType.Name;
			//oldPackage = SimpleDataType.Package;
			//oldParent = SimpleDataType.Parent;
			oldXSD = SimpleDataType.DefaultXSDImplementation;
			
			SimpleDataType.Name = Name;
			//SimpleDataType.Package = ;
			//SimpleDataType.Parent = ;
			SimpleDataType.DefaultXSDImplementation = XSDImplementation;

		}

		internal override OperationResult UndoOperation()
		{
			SimpleDataType.Name = oldName;
			//SimpleDataType.Package = oldPackage;
			//SimpleDataType.Parent = oldParent;
			SimpleDataType.DefaultXSDImplementation = oldXSD;
			return OperationResult.OK;
		}
	}

	#region AlterSimpleTypeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="AlterSimpleTypeCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class AlterSimpleTypeCommandFactory : ModelCommandFactory<AlterSimpleTypeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private AlterSimpleTypeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of AlterSimpleTypeCommand
		/// <param name="modelController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new AlterSimpleTypeCommand(modelController);
		}
	}

	#endregion
}
