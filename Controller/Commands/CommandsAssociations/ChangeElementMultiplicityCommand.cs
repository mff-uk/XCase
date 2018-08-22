using NUml.Uml2;
using MultiplicityElement = XCase.Model.MultiplicityElement;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Sets new multiplicity to an end af an association.
	/// </summary>
    public class ChangeElementMultiplicityCommand : ModelCommandBase
    {
    	public ChangeElementMultiplicityCommand(ModelController controller)
    		: base(controller)
    	{
    		Description = CommandDescription.CHANGE_MULTIPLICITY;
    	}

		/// <summary>
		/// Modified element
		/// </summary>
		[MandatoryArgument]
        public MultiplicityElement Element { get; set; }

        private uint ? oldLower { get; set; }
        
		private UnlimitedNatural ? oldUpper { get; set; }

		/// <summary>
		/// Lower multiplicity bound
		/// </summary>
		public uint ? Lower { get; set; }
    	
		/// <summary>
		/// Upper multiplicity bound
		/// </summary>
		public UnlimitedNatural ? Upper { get; set; }

        public override bool CanExecute()
        {
			if (Lower.HasValue && Lower > Upper)
        	{
        		ErrorDescription = CommandError.CMDERR_MULTIPLICITY_BAD_BOUNDS;
        		return false; 
        	}
        	return true;
        }

        internal override void CommandOperation()
        {
			oldLower = Element.Lower;
            oldUpper = Element.Upper;
			Element.Lower = Lower;
        	if (Upper.HasValue) 
				Element.Upper = Upper.Value;
        }

    	internal override OperationResult UndoOperation()
        {
            Element.Lower = oldLower;
            if (Upper.HasValue)
				Element.Upper = oldUpper.Value;
            return OperationResult.OK;
        }
    }

	#region ChangeElementMultiplicityCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ChangeElementMultiplicityCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class ChangeElementMultiplicityCommandFactory : ModelCommandFactory<ChangeElementMultiplicityCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ChangeElementMultiplicityCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ChangeElementMultiplicityCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new ChangeElementMultiplicityCommand(modelController);
		}
	}

	#endregion
}