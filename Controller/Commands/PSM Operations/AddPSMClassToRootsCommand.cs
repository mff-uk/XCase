using System;
using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds this Class to Roots collection of a PSM Diagram.
    /// </summary>
    public class AddPSMClassToRootsCommand : DiagramCommandBase
    {
        [MandatoryArgument]
        private ElementHolder<PSMClass> pSMClassHolder { get; set; }

        public AddPSMClassToRootsCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.ADD_PSM_DIAGRAM_ROOT;
        }

        public void Set(ElementHolder<PSMClass> ch)
        {
            pSMClassHolder = ch;
        }

        public override bool CanExecute()
        {
			if (pSMClassHolder != null && pSMClassHolder.HasValue && pSMClassHolder.Element.Diagram != Diagram)
			{
				ErrorDescription = CommandError.CMDERR_ROOT_CLASS_WRONG_DIAGRAM;
				return false;
			
			}
            return pSMClassHolder != null;
        }

        internal override void CommandOperation()
        {
			AssociatedElements.Add(pSMClassHolder.Element);
        	
            (Diagram as PSMDiagram).Roots.Add(pSMClassHolder.Element);
        }

        internal override OperationResult UndoOperation()
        {
            (Diagram as PSMDiagram).Roots.Remove(pSMClassHolder.Element);
            return OperationResult.OK;
        }

    }

    #region AddPSMClassToRootsCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="AddPSMClassToRootsCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class AddPSMClassToRootsCommandFactory : DiagramCommandFactory<AddPSMClassToRootsCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private AddPSMClassToRootsCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of AddPSMClassToRootsCommand
        /// <param name="controller">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController controller)
        {
            return new AddPSMClassToRootsCommand(controller) { };
        }
    }

    #endregion
}
