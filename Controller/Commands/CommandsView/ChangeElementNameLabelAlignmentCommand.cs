using XCase.Model;
using System.Windows;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Sets the Aligned to right property of PSM Classes ElementName label
    /// </summary>
    public class ChangeElementNameLabelAlignmentCommand : ViewCommand
    {
        [MandatoryArgument]
        private ClassViewHelper ViewHelper { get; set; }

        [MandatoryArgument]
        private bool AlignedRight { get; set; }

        private bool OriginalAlignedRight;
        
        public ChangeElementNameLabelAlignmentCommand(DiagramController controller)
            : base(controller)
        {
            Description = CommandDescription.ALIGN_ELEMENT_NAME;
        }

        public void Set(ClassViewHelper viewHelper, bool alignedRight)
        {
            ViewHelper = viewHelper;
            AlignedRight = alignedRight;
            OriginalAlignedRight = viewHelper.ElementNameLabelAlignedRight;
        }

        public override bool CanExecute()
        {
            return ViewHelper != null;
        }

        internal override void CommandOperation()
        {
            ViewHelper.ElementNameLabelAlignedRight = AlignedRight;
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            ViewHelper.ElementNameLabelAlignedRight = OriginalAlignedRight;
            return OperationResult.OK;
        }

    }

    #region ChangeElementNameLabelAlignmentCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="ChangeElementNameLabelAlignmentCommand"/>.
    /// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="DiagramController"/> during their initialization.
    /// </summary>
    public class ChangeElementNameLabelAlignmentCommandFactory : DiagramCommandFactory<ChangeElementNameLabelAlignmentCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private ChangeElementNameLabelAlignmentCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of ChangeElementNameLabelAlignmentCommand
        /// <param name="diagramController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(DiagramController diagramController)
        {
            return new ChangeElementNameLabelAlignmentCommand(diagramController);
        }
    }

    #endregion
}
