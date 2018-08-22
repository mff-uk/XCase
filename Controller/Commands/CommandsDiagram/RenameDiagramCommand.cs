using System;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Renames a Diagram
    /// </summary>
    public class RenameDiagramCommand : DiagramCommandBase
    {
        [MandatoryArgument]
        public Model.Diagram RenamedDiagram { get; set; }

        [MandatoryArgument]
        public string NewCaption { get; set; }

        [MandatoryArgument]
        public Project Project { get; set; }

    	private string oldCaption; 

        public RenameDiagramCommand(DiagramController diagramController)
            : base(diagramController)
        {
            Description = CommandDescription.RENAME_DIAGRAM;
        }

        public override bool CanExecute()
        {
            if (!NameSuggestor<Diagram>.IsNameUnique(Project.Diagrams, NewCaption, diagram => diagram.Caption))
            {
                ErrorDescription = String.Format(CommandError.CMDERR_NAME_NOT_UNIQUE, NewCaption);
                return false;
            }
            return RenamedDiagram != null && NewCaption != null;
        }

        internal override void CommandOperation()
        {
        	oldCaption = RenamedDiagram.Caption;
        	RenamedDiagram.Caption = NewCaption;
        }

        internal override OperationResult UndoOperation()
        {
        	RenamedDiagram.Caption = oldCaption;
        	return OperationResult.OK;
        }
    }

	#region RenameDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="RenameDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class RenameDiagramCommandFactory : DiagramCommandFactory<RenameDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private RenameDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of RenameDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new RenameDiagramCommand(diagramController);
		}
	}

	#endregion
}
