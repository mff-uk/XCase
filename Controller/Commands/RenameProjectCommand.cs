using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Renames a project
    /// </summary>
    public class RenameProjectCommand : ModelCommandBase
    {
        private string OldName { get; set; }
        private string NewName { get; set; }
        private Project Project;

        public RenameProjectCommand(ModelController controller)
            : base(controller)
        { }

        public void Set(Project project, string newName)
        {
            NewName = newName;
            Project = project;
        }
        
        // check if the new name is unique when implementing more than one project
        public override bool CanExecute()
        {
            return Project != null;
        }

        internal override void CommandOperation()
        {
            OldName = Project.Caption;
            Project.Caption = NewName;
        }

        internal override OperationResult UndoOperation()
        {
            Project.Caption = OldName;
            return OperationResult.OK;
        }
    }
    #region RenameProjectCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RenameProjectCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class RenameProjectCommandFactory : ModelCommandFactory<RenameProjectCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RenameProjectCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of RenameProjectCommand
		/// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new RenameProjectCommand(modelController);
        }
    }
    #endregion
}