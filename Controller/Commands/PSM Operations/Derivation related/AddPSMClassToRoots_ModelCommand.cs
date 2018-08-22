using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds this Class to Roots collection of a PSM Diagram. Intended for MacroCommand use, 
    /// i.e. derive PSM Class, create new PSM Diagram, add the class to the diagram and make it root.
    /// This is the reason this command is Model Command, because there is no way to get to the
    /// newly created DiagramController.
    /// </summary>
    public class AddPSMClassToRoots_ModelCommand : ModelCommandBase
    {
        [MandatoryArgument]
        private ElementHolder<PSMClass> pSMClassHolder { get; set; }

        [MandatoryArgument]
        private HolderBase<PSMDiagram> DiagramHolder { get; set; }

        public AddPSMClassToRoots_ModelCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.ADD_PSM_DIAGRAM_ROOT;
        }

        public void Set(ElementHolder<PSMClass> ch, HolderBase<PSMDiagram> dh)
        {
            pSMClassHolder = ch;
            DiagramHolder = dh;
        }

        public override bool CanExecute()
        {
            return pSMClassHolder != null && DiagramHolder != null;
        }

        internal override void CommandOperation()
        {
			AssociatedElements.Add(pSMClassHolder.Element);
            DiagramHolder.Element.Roots.Add(pSMClassHolder.Element);
        }

        internal override OperationResult UndoOperation()
        {
            DiagramHolder.Element.Roots.Remove(pSMClassHolder.Element);
            return OperationResult.OK;
        }

    }

    #region AddPSMClassToRoots_ModelCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="AddPSMClassToRoots_ModelCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class AddPSMClassToRoots_ModelCommandFactory : ModelCommandFactory<AddPSMClassToRoots_ModelCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private AddPSMClassToRoots_ModelCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of AddPSMClassToRoots_ModelCommand
        /// <param name="controller">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController controller)
        {
            return new AddPSMClassToRoots_ModelCommand(controller) { };
        }
    }

    #endregion
}
