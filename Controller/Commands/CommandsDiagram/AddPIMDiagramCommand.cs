using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds a PIM Diagram
    /// </summary>
    public class AddPIMDiagramCommand : ModelCommandBase
    {
        [MandatoryArgument]
        public HolderBase<PIMDiagram> DiagramHolder { get; private set; }

        public PIMDiagram Diagram
        {
            get { return DiagramHolder.Element; }
            private set { DiagramHolder.Element = value; }
        }

        [MandatoryArgument]
        private Project Project { get; set; }

        public AddPIMDiagramCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.ADD_PIM_DIAGRAM;
        }

        public void Set(Project project, HolderBase<PIMDiagram> h)
        {
            Project = project;
            if (h == null) h = new HolderBase<PIMDiagram>();
            DiagramHolder = h;
        }

        public override bool CanExecute()
        {
            return Project != null;
        }

        internal override void CommandOperation()
        {
            if (Diagram == null) Diagram = new PIMDiagram(NameSuggestor<PIMDiagram>.SuggestUniqueName(Project.PIMDiagrams, "PIM Diagram", diagram => diagram.Caption));
            Project.AddDiagram(Diagram);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            Project.RemoveDiagram(Diagram);
            return OperationResult.OK;
        }
    }
    #region AddPIMDiagramCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="AddPIMDiagramCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class AddPIMDiagramCommandFactory : ModelCommandFactory<AddPIMDiagramCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private AddPIMDiagramCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="AddPIMDiagramCommand"/>
        /// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new AddPIMDiagramCommand(modelController);
        }
    }
    #endregion
}
