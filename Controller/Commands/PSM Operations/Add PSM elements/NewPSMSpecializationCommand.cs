using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds a specialization between 2 PSM classes
    /// </summary>
    public class NewPSMSpecializationCommand : ModelCommandBase
    {
        /// <summary>
		/// Creates new instance of <see cref="AddPSMSpecializationCommand">AddPSMSpecificationCommand</see>. 
		/// </summary>
		/// <param name="modelController">command controller</param>
        public NewPSMSpecializationCommand(ModelController modelController)
			: base(modelController)
		{
			Description = CommandDescription.PSM_ADD_SPECIFICATIONS;
		}

		[MandatoryArgument]
        public ElementHolder<PSMClass> GeneralPSMClass { get; set; }

		[MandatoryArgument]
        public ElementHolder<PSMClass> SpecificPSMClass { get; set; }

		[CommandResult]
		public ElementHolder<Generalization> CreatedGeneralization { get; set;}

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			Generalization generalization = Controller.Model.Schema.SetGeneralization(GeneralPSMClass.Element, SpecificPSMClass.Element);
			if (CreatedGeneralization == null)
				CreatedGeneralization = new ElementHolder<Generalization>();
			CreatedGeneralization.Element = generalization;
		}

		internal override OperationResult UndoOperation()
		{
			CreatedGeneralization.Element.RemoveMeFromModel();
			return OperationResult.OK;
		}

        internal override void RedoOperation()
        {
            CreatedGeneralization.Element.PutMeBackToModel();
        }
	}

	#region AddPSMSpecializationCommandFactory

	/// <summary>
    /// Factory that creates instances of <see cref="NewPSMSpecializationCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
    public class NewPSMSpecializationCommandFactory : ModelCommandFactory<NewPSMSpecializationCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
        private NewPSMSpecializationCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of NewPSMSpecializationCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
            return new NewPSMSpecializationCommand(modelController);
		}
	}

	#endregion
}
