using System;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Renames the XML Namespace of this project
    /// </summary>
    public class RenameProjectNamespaceCommand : ModelCommandBase
    {
        [MandatoryArgument]
        public string NewNamespaceName { get; set; }

    	private string oldNamespaceName;

        public RenameProjectNamespaceCommand(ModelController Controller)
            : base(Controller)
        {
            Description = CommandDescription.RENAME_PROJECT_NAMESPACE;
        }

        public override bool CanExecute()
        {
        	return true; 
        }

        internal override void CommandOperation()
        {
            oldNamespaceName = Controller.Project.Schema.XMLNamespace;
            Controller.Project.Schema.XMLNamespace = NewNamespaceName;
        }

        internal override OperationResult UndoOperation()
        {
            Controller.Project.Schema.XMLNamespace = oldNamespaceName;
        	return OperationResult.OK;
        }
    }

    #region RenameProjectNamespaceCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="RenameProjectNamespaceCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class RenameProjectNamespaceCommandFactory : ModelCommandFactory<RenameProjectNamespaceCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
        private RenameProjectNamespaceCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of RenameProjectNamespaceCommand
		/// <param name="Controller">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController Controller)
		{
            return new RenameProjectNamespaceCommand(Controller);
		}
	}

	#endregion
}
