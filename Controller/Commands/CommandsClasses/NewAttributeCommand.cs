using System;
using NUml.Uml2;
using XCase.Controller.Commands.Helpers;
using XCase.Model;
using DataType=XCase.Model.DataType;
using Property=XCase.Model.Property;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Creates new attribute for an element implementing <see cref="IHasAttributes"/> (usually Class).
	/// </summary>
    public class NewAttributeCommand : ModelCommandBase
    {
		/// <summary>
		/// Creates new instance of <see cref="NewAttributeCommand" />. 
		/// </summary>
		/// <param name="controller">command controller</param>
		public NewAttributeCommand(ModelController controller)
			: base(controller)
		{
			Description = CommandDescription.ADD_ATTRIBUTE;
			Lower = 1;
			Upper = 1;
		}

        /// <summary>
        /// Element holder for the created attribute
        /// </summary>
        public ElementHolder<Property> createdAttributeHolder { set; private get; }
        
        /// <summary>
		/// New added attribute
		/// </summary>
		[CommandResult]
		public Property createdAttribute { get; set; }

		/// <summary>
		/// Element where attribute is added
		/// </summary>
		[MandatoryArgument]
		public IHasAttributes Owner { get; set; }

		/// <summary>
		/// Name of the new attribute
		/// </summary>
		[MandatoryArgument]
		public string Name { get; set; }

		/// <summary>
		/// Type of the new attribute
		/// </summary>
		public ElementHolder<DataType> Type { get; set; }

		/// <summary>
		/// Default value of the created attribute
		/// </summary>
		public string Default { get; set; }

		/// <summary>
		/// Upper multiplicity bound of the created attribute, default is 1
		/// </summary>
		public UnlimitedNatural ? Upper { get; set; }

		/// <summary>
		/// Lower multiplicity bound of the created attribute, default is 1
		/// </summary>
		public uint ? Lower { get; set; }

        /// <summary>
        /// Index from which the attribute is removed in Undo
        /// </summary>
        private int Index;

		public override bool CanExecute()
		{
			if (Lower != null && Upper != null)
			{
				if (Lower > Upper)
				{
					ErrorDescription = CommandError.CMDERR_MULTIPLICITY_BAD_BOUNDS;
					return false; 
				}
			}
			if (!NameSuggestor<Property>.IsNameUnique(Owner.Attributes, Name, attribute => attribute.Name))
			{
				ErrorDescription = String.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, Name);
				return false; 
			}
			return true;
		}

		internal override void CommandOperation()
		{
			if (Owner is Model.Element)
				AssociatedElements.Add((Model.Element)Owner);
			createdAttribute = Owner.AddAttribute();
            if (createdAttributeHolder != null) createdAttributeHolder.Element = createdAttribute;
			createdAttribute.Name = Name;
			createdAttribute.Lower = Lower;
			if (Type != null && Type.Element != null)
				createdAttribute.Type = Type.Element;
			if (Upper.HasValue) 
				createdAttribute.Upper = Upper.Value;
			createdAttribute.Default = Default;
		}

		internal override OperationResult UndoOperation()
		{
            if (!Owner.Attributes.Contains(createdAttribute))
            {
                ErrorDescription = CommandError.CMDERR_REMOVING_DETACHED_ATTRIBUTE;
                return OperationResult.Failed;
            }
            else if (createdAttribute.DerivedPSMAttributes.Count > 0)
            {
                ErrorDescription = string.Format(CommandError.CMDERR_DELETE_PSM_DEPENDENT_ATTRIBUTE, createdAttribute);
                return OperationResult.Failed;
            }
            else
            {
                Index = Owner.Attributes.IndexOf(createdAttribute);
                Owner.Attributes.Remove(createdAttribute);
                return OperationResult.OK;
            }
		}

        internal override void RedoOperation()
        {
            Owner.Attributes.Insert(Index, createdAttribute);
        }
    }

	#region NewAttributeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewAttributeCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class NewAttributeCommandFactory : ModelCommandFactory<NewAttributeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewAttributeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewAttributeCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new NewAttributeCommand(modelController);
		}
	}

	#endregion
}