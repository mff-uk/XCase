using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds a PSM Diagram
    /// </summary>
    public class AddPSMDiagramCommand : ModelCommandBase
    {
        [MandatoryArgument]
        public HolderBase<PSMDiagram> DiagramHolder { get; private set; }

        public PSMDiagram Diagram
        {
            get { return DiagramHolder.Element; }
            private set { DiagramHolder.Element = value; }
        }

        [MandatoryArgument]
        private Project Project { get; set; }

        public AddPSMDiagramCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.ADD_PSM_DIAGRAM;
        }

        public void Set(Project project, HolderBase<PSMDiagram> h)
        {
            Project = project;
            if (h == null) h = new HolderBase<PSMDiagram>();
            DiagramHolder = h;
        }

        public override bool CanExecute()
        {
            return Project != null;
        }

        internal override void CommandOperation()
        {
            if (Diagram == null) Diagram = new PSMDiagram(NameSuggestor<PSMDiagram>.SuggestUniqueName(Project.PSMDiagrams, "PSM Diagram", diagram => diagram.Caption));
            Project.AddDiagram(Diagram);
        }

        internal override OperationResult UndoOperation()
        {
            Project.RemoveDiagram(Diagram);
            return OperationResult.OK;
        }
    }
    #region AddPSMDiagramCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="AddPSMDiagramCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands receive reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class AddPSMDiagramCommandFactory : ModelCommandFactory<AddPSMDiagramCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private AddPSMDiagramCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="AddPSMDiagramCommand"/>
		/// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new AddPSMDiagramCommand(modelController);
        }
    }
    #endregion
}
