using System;
using System.Collections.Generic;
using XCase.Model;
using System.Linq;
using DataType=XCase.Model.DataType;
using Property=XCase.Model.Property;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds attribute to a PSM Class
	/// </summary>
	public class AddPSMClassAttributeCommand : DiagramCommandBase
	{
		/// <summary>
		/// Class where attributes are included
		/// </summary>
		[MandatoryArgument]
		public PSMClass PSMClass { get; set; }

		/// <summary>
		/// Attribute of a PIM class. If used, created attribute will
		/// represent this attribute. If not used (left to <c>null</c>), attribute
		/// will be created as pim-less. 
		/// </summary>
        public Property RepresentedAttribute { get; set; }

        [CommandResult]
        public PSMAttribute CreatedAttribute { get; set; }

        public string Alias { get; set; }

        public string Name { get; set; }

        private string DecideName()
        {
            if (!String.IsNullOrEmpty(Alias))
            {
                return Alias;
            }
            if (Name != null)
            {
                return Name; 
            }
            if (RepresentedAttribute != null)
            {
                return RepresentedAttribute.Name;
            }
            return null; 
        }

        public DataType Type { get; set; }

	    private bool customMultiplicity;

	    private uint? lower;
	    public uint? Lower
	    {
	        get { return lower; }
	        set { lower = value;
	            customMultiplicity = true; }
	    }

	    private NUml.Uml2.UnlimitedNatural upper;
	    public NUml.Uml2.UnlimitedNatural Upper
	    {
	        get { return upper; }
	        set { upper = value;
	            customMultiplicity = true; }
	    }

	    public string Default { get; set; }

	    /// <summary>
        /// Creates new instance of <see cref="AddPSMClassAttributeCommand">AddPSMClassAttributeCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
        public AddPSMClassAttributeCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_ADD_ATTRIBUTE;
		}

        public IEnumerable<string> UsedAliasesOrNames { get; set; }

	    public override bool CanExecute()
		{
            if (RepresentedAttribute != null)
            {
                if (!PSMClass.RepresentedClass.MeAndAncestors.Contains(RepresentedAttribute.Class))
                {
                    ErrorDescription = CommandError.CMDERR_INCLUDED_ATTRIBUTES_INCONSISTENCE;
                    return false;
                }

                if (Name != RepresentedAttribute.Name || Type != null)
                {
                    ErrorDescription = CommandError.CMDERR_REPRESENTING_ATTRIBUTES_CAN_NOT_BE_UPDATED;
                }
            }

		    string name = DecideName();
		    if (name == null)
		    {
                ErrorDescription = CommandError.CMDERR_PIMLESS_NAME;
		    }

	        IEnumerable<string> nameCollection;
	        if (UsedAliasesOrNames != null)
	        {
	            nameCollection = UsedAliasesOrNames;
	        }
            else
	        {
	            nameCollection = from PSMAttribute att in PSMClass.PSMAttributes select att.AliasOrName;
	        }

            if (!NameSuggestor<string>.IsNameUnique(nameCollection, name, n => n))
			{
                ErrorDescription = string.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, name);
				return false; 
			}

            if (!MultiplicityElementController.IsMultiplicityValid(Lower, Upper))
            {
                ErrorDescription = string.Format(CommandError.CMDERR_MULTIPLICITY_BAD_BOUNDS, name);
                return false;
            }
			return true;
		}

	    internal override void CommandOperation()
		{
			PSMAttribute psmAttribute;

	        if (RepresentedAttribute != null)
	        {
	            psmAttribute = PSMClass.AddAttribute(RepresentedAttribute);
	            psmAttribute.Lower = RepresentedAttribute.Lower;
	            psmAttribute.Upper = RepresentedAttribute.Upper;
	        }
            else
	        {
	            psmAttribute = PSMClass.AddAttribute((Property)null);
                if (!String.IsNullOrEmpty(Name))
                    psmAttribute.Name = Name;
                else if (!String.IsNullOrEmpty(Alias))
                    psmAttribute.Name = Alias;
                else
                    psmAttribute.Name = NameSuggestor<PSMAttribute>.SuggestUniqueName(PSMClass.PSMAttributes, "Attribute", a => a.AliasOrName);

	            psmAttribute.Type = Type;
	        }

            psmAttribute.Alias = Alias;
                
			if (customMultiplicity)
			{
			    psmAttribute.Lower = Lower;
			    psmAttribute.Upper = Upper;
			}

	        psmAttribute.Default = Default;

			AssociatedElements.Add(PSMClass);
            CreatedAttribute = psmAttribute;
		}


		internal override OperationResult UndoOperation()
		{
            if (CreatedAttribute.RepresentedAttribute != null)
            {
                CreatedAttribute.RepresentedAttribute.DerivedPSMAttributes.Remove(CreatedAttribute);
            }
            PSMClass.Attributes.Remove(CreatedAttribute);
			
			return OperationResult.OK;
		}

        internal override void RedoOperation()
        {
            PSMClass.Attributes.Add(CreatedAttribute);
            if (CreatedAttribute.RepresentedAttribute != null)
            {
                CreatedAttribute.RepresentedAttribute.DerivedPSMAttributes.Add(CreatedAttribute);
            }
        }
	}

	#region AddPSMClassAttributeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="AddPSMClassAttributeCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class AddPSMClassAttributeCommandFactory : DiagramCommandFactory<AddPSMClassAttributeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private AddPSMClassAttributeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of <see cref="AddPSMClassAttributeCommand"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new AddPSMClassAttributeCommand(diagramController);
		}
	}
	#endregion
}