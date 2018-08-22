using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds a PSM Class to PSM Diagram without the knowledge of PSM Diagrams DiagramController. 
    /// This is useful for cross-diagram MacroCommands.
    /// </summary>
    public class PSMClassToDiagram_ModelCommand : ModelCommandBase
    {
        [MandatoryArgument]
        private ElementHolder<PSMClass> pSMClassHolder {get; set;}

        private PSMElementViewHelper ViewHelper;

        [MandatoryArgument]
        private HolderBase<PSMDiagram> DiagramHolder { get; set; }

        public PSMClassToDiagram_ModelCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.PSM_CLASS_TO_DIAGRAM_MODELCOMMAND;
        }

        public void Set(ElementHolder<PSMClass> c, HolderBase<PSMDiagram> d)
        {
            pSMClassHolder = c;
            DiagramHolder = d;
        }
        
        public override bool CanExecute()
        {
            return DiagramHolder != null && pSMClassHolder != null;
        }

        internal override void CommandOperation()
        {
			AssociatedElements.Add(pSMClassHolder.Element);
			if (ViewHelper == null) ViewHelper = new PSMElementViewHelper(DiagramHolder.Element) { X = 0, Y = 0, Height = double.NaN, Width = double.NaN };
            DiagramHolder.Element.AddModelElement(pSMClassHolder.Element, ViewHelper);
            pSMClassHolder.Element.Diagram = DiagramHolder.Element;
        }

        internal override OperationResult UndoOperation()
        {
            DiagramHolder.Element.RemoveModelElement(pSMClassHolder.Element);
            return OperationResult.OK;
        }
    }

    #region PSMClassToDiagram_ModelCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="PSMClassToDiagram_ModelCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class PSMClassToDiagram_ModelCommandFactory : ModelCommandFactory<PSMClassToDiagram_ModelCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private PSMClassToDiagram_ModelCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of PSMClassToDiagram_ModelCommand
        /// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new PSMClassToDiagram_ModelCommand(modelController);
        }
    }

    #endregion
}
