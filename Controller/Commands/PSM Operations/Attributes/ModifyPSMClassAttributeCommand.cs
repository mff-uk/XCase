using XCase.Model;
using DataType=XCase.Model.DataType;
using Property=XCase.Model.Property;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds attribute to a PSM Class
	/// </summary>
	public class ModifyPSMClassAttributeCommand : DiagramCommandBase
	{
		[MandatoryArgument]
		public PSMAttribute PSMAttribute { get; set; }

        public string Alias { get; set; }

        public string Name { get; set; }

        private string DecideName()
        {
            if (Name != null)
            {
                return Name; 
            }
            else 
            {
                return PSMAttribute.Name;
            }
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
        /// Creates new instance of <see cref="ModifyPSMClassAttributeCommand">ModifyPSMClassAttributeCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
        public ModifyPSMClassAttributeCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_ADD_ATTRIBUTE;
		}

	    public override bool CanExecute()
		{
            if (PSMAttribute.RepresentedAttribute != null)
            {
                if (Name != PSMAttribute.RepresentedAttribute.Name || Type != PSMAttribute.RepresentedAttribute.Type)
                {
                    ErrorDescription = CommandError.CMDERR_REPRESENTING_ATTRIBUTES_CAN_NOT_BE_UPDATED;
                }
            }

		    string name = DecideName();
		    if (name == null)
		    {
                ErrorDescription = CommandError.CMDERR_PIMLESS_NAME;
		    }

            if (!NameSuggestor<PSMAttribute>.IsNameUnique(PSMAttribute.Class.PSMAttributes, name, attribute => attribute.AliasOrName, PSMAttribute))
			{
                ErrorDescription = string.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, name);
				return false; 
			}

            if (customMultiplicity && !MultiplicityElementController.IsMultiplicityValid(Lower, Upper))
            {
                ErrorDescription = string.Format(CommandError.CMDERR_MULTIPLICITY_BAD_BOUNDS, name);
                return false; 
            }
		
			return true;
		}

	    private string oldName; 
	    private DataType oldType;
	    private string oldAlias;
	    private string oldDefault;
	    private uint? oldLower;
	    private NUml.Uml2.UnlimitedNatural oldUpper; 
	    internal override void CommandOperation()
		{
	        oldType = PSMAttribute.Type;
			PSMAttribute.Type = Type;

	        oldAlias = PSMAttribute.Alias;
            if (Alias != null)
                PSMAttribute.Alias = Alias;

	        oldLower = PSMAttribute.Lower;
	        oldUpper = PSMAttribute.Upper;

			if (customMultiplicity)
			{
                PSMAttribute.Lower = Lower;
                PSMAttribute.Upper = Upper;
			}

	        oldDefault = PSMAttribute.Default;
	        PSMAttribute.Default = Default;
            
            oldName = PSMAttribute.Name;
	        PSMAttribute.Name = Name;

            AssociatedElements.Add(PSMAttribute.Class);
		}


		internal override OperationResult UndoOperation()
		{
		    PSMAttribute.Type = oldType;
		    PSMAttribute.Alias = oldAlias;
		    PSMAttribute.Lower = oldLower;
		    PSMAttribute.Upper = oldUpper;
		    PSMAttribute.Default = oldDefault;
		    PSMAttribute.Name = oldName;
			
			return OperationResult.OK;
		}
	}

	#region ModifyPSMClassAttributeCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ModifyPSMClassAttributeCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands receive reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ModifyPSMClassAttributeCommandFactory : DiagramCommandFactory<ModifyPSMClassAttributeCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ModifyPSMClassAttributeCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of <see cref="ModifyPSMClassAttributeCommand"/>
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ModifyPSMClassAttributeCommand(diagramController);
		}
	}
	#endregion
}