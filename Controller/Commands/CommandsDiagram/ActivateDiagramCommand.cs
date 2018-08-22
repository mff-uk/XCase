using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// This is a "fake" command that causes diagram activation (Gui needs to detect this
    /// command through CommandExecuted event in ModelController)
    /// Intended to be used in MacroCommands since it has no UndoOperation but it would
    /// still appear in the Undo stack
    /// </summary>
    public class ActivateDiagramCommand : ModelCommandBase
    {
        /// <summary>
        /// Diagram to be activated
        /// </summary>
        public Diagram ActivatedDiagram { get; private set; }

        public ActivateDiagramCommand(ModelController controller)
            : base(controller)
        {
            Undoable = false;
        }

        /// <summary>
        /// Diagram to be activated
        /// </summary>
        /// <param name="d"></param>
        public void Set(Diagram d)
        {
            ActivatedDiagram = d;
        }

        /// <summary>
        /// If this property is used, specified element should be selected on the diagram.
        /// </summary>
        public Element Element { get; set; }

        public override bool CanExecute()
        {
            return ActivatedDiagram != null || Element != null;
        }

        internal override void CommandOperation()
        {
        }

        internal override OperationResult UndoOperation()
        {
            return OperationResult.OK;
        }

    }

    #region ActivateDiagramCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="ActivateDiagramCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class ActivateDiagramCommandFactory : ModelCommandFactory<ActivateDiagramCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private ActivateDiagramCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="ActivateDiagramCommand"/>
		/// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new ActivateDiagramCommand(modelController);
        }
    }
    #endregion

}
