using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Removes diagram from a project.
    /// </summary>
    public class RemoveDiagramCommand : ModelCommandBase
    {
        /// <summary>
        /// Diagram to be removed from a project.
        /// </summary>
        [MandatoryArgument]
        private Diagram RemovedDiagram { get; set; }

        /// <summary>
        /// The project from which the diagram should be removed.
        /// </summary>
        [MandatoryArgument]
        private Project Project { get; set; }
        
        public RemoveDiagramCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_DIAGRAM;
        }

        public void Set(Project project, Diagram removedDiagram)
        {
            Project = project;
            RemovedDiagram = removedDiagram;
        }

        public override bool CanExecute()
        {
            return (RemovedDiagram != null & Project != null);
        }

        internal override void CommandOperation()
        {
            Project.RemoveDiagram(RemovedDiagram);
        }

        internal override OperationResult UndoOperation()
        {
            Project.AddDiagram(RemovedDiagram);
            return OperationResult.OK;
        }
    }

    #region RemoveDiagramCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RemoveDiagramCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class RemoveDiagramCommandFactory : ModelCommandFactory<RemoveDiagramCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RemoveDiagramCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of RemoveDiagramCommand
		/// <param name="modelController">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new RemoveDiagramCommand(modelController);
        }
    }
    #endregion
}
