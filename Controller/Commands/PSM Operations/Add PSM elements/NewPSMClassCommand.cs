using System;
using System.Diagnostics;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>   
    /// Creates a new PSM class that represents a given PIM class from the 
    /// model and is a part of the same package as the PIM class.
    /// </summary>
    public class NewPSMClassCommand : ModelCommandBase
    {
        /// <summary>
        /// A PIM class represented by the new PSM class.
        /// </summary>
        public PIMClass RepresentedClass { get; set; }

        /// <summary>
		/// An elementHolder, where the reference to the newly created class can be stored.
		/// </summary>
        public ElementHolder<PSMClass> CreatedClass { get; set; }

    	public NewPSMClassCommand(ModelController modelController)
            : base(modelController) 
        {
            Description = CommandDescription.ADD_PSM_CLASS;
        }

        /// <summary>
        /// Checks whether representedClass is not null.
        /// </summary>
    	public override bool CanExecute()
        {
            return ( RepresentedClass != null );
        }

    	internal override void CommandOperation()
        {
			if (CreatedClass == null)
				CreatedClass = new ElementHolder<PSMClass>();

            PSMClass psmClass = RepresentedClass.DerivePSMClass();

            CreatedClass.Element = psmClass;
			
            Debug.Assert(CreatedClass.HasValue);
			AssociatedElements.Add(CreatedClass.Element);
        }

    	internal override OperationResult UndoOperation()
        {
            Debug.Assert(CreatedClass.HasValue);
            CreatedClass.Element.RemoveMeFromModel();
            return OperationResult.OK;
        }

        internal override void RedoOperation()
        {
            CreatedClass.Element.PutMeBackToModel();
        }
    }

    #region NewPSMClassCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="NewPSMClassCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class NewPSMClassCommandFactory : ModelCommandFactory<NewPSMClassCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private NewPSMClassCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of NewPSMClassCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new NewPSMClassCommand(modelController);
        }
    }

    #endregion
}
