using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Renames a project
    /// </summary>
    public class RenameVersionCommand : ModelCommandBase
    {
        private string OldName { get; set; }
        private string NewName { get; set; }
        private Version Version;

        public RenameVersionCommand(ModelController controller)
            : base(controller)
        { }

        public void Set(Version version, string newName)
        {
            NewName = newName;
            Version = version;
        }
        
        // check if the new name is unique when implementing more than one project
        public override bool CanExecute()
        {
            return Version != null;
        }

        internal override void CommandOperation()
        {
            OldName = Version.Label;
            Version.Label = NewName;
        }

        internal override OperationResult UndoOperation()
        {
            Version.Label = OldName;
            return OperationResult.OK;
        }
    }


    #region RenameVersionCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RenameVersionCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class RenameVersionCommandFactory : ModelCommandFactory<RenameVersionCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RenameVersionCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of RenameVersionCommand
        /// <param name="modelController">Associated diagram controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController modelController)
        {
            return new RenameVersionCommand(modelController);
        }
    }
    #endregion
}