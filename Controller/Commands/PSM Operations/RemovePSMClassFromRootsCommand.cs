using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Removes this Class from Roots collection of a PSM Diagram.
    /// </summary>
    public class RemovePSMClassFromRootsCommand : DiagramCommandBase
    {
        [MandatoryArgument]
        private ElementHolder<PSMClass> pSMClassHolder { get; set; }

        public RemovePSMClassFromRootsCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_PSMCLASS_FROM_ROOTS;
        }

        public void Set(ElementHolder<PSMClass> ch)
        {
            pSMClassHolder = ch;
        }

        public override bool CanExecute()
        {
            return pSMClassHolder != null;
        }

        internal override void CommandOperation()
        {
			AssociatedElements.Add(pSMClassHolder.Element);
            (Diagram as PSMDiagram).Roots.Remove(pSMClassHolder.Element);
        }

        internal override OperationResult UndoOperation()
        {
            (Diagram as PSMDiagram).Roots.Add(pSMClassHolder.Element);
            return OperationResult.OK;
        }

    }

    #region RemovePSMClassFromRootsCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RemovePSMClassFromRootsCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class RemovePSMClassFromRootsCommandFactory : DiagramCommandFactory<RemovePSMClassFromRootsCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RemovePSMClassFromRootsCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of RemovePSMClassFromRootsCommand
        /// <param name="controller">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController controller)
        {
            return new RemovePSMClassFromRootsCommand(controller) { };
        }
    }

    #endregion
}
