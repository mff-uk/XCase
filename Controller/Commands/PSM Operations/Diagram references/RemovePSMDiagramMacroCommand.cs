using XCase.Model;
using System.Linq;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Removes diagram from a project.
    /// </summary>
    public class RemovePSMDiagramMacroCommand : MacroCommand<ModelController>
    {
        /// <summary>
        /// Diagram to be removed from a project.
        /// </summary>
        [MandatoryArgument]
        private PSMDiagram RemovedDiagram { get; set; }

        /// <summary>
        /// The project from which the diagram should be removed.
        /// </summary>
        [MandatoryArgument]
        private Project Project { get; set; }

        public RemovePSMDiagramMacroCommand(ModelController controller)
            : base(controller)
        {
            Description = CommandDescription.REMOVE_DIAGRAM;
        }

        public void Set(Project project, PSMDiagram removedDiagram, DiagramController DiagramController)
        {
			DeleteFromPSMDiagramConsideringRepresentativesMacroCommand delete = (DeleteFromPSMDiagramConsideringRepresentativesMacroCommand)DeleteFromPSMDiagramConsideringRepresentativesMacroCommandFactory.Factory().Create(DiagramController);
        	delete.ForceDelete = true; 
			if (!delete.InitializeCommand(null, removedDiagram.Roots.Cast<Element>().ToArray()))
            {
                Commands.Clear();
                return;
            }
        	
            Commands.Add(delete);
            
            RemoveDiagramCommand c = (RemoveDiagramCommand)RemoveDiagramCommandFactory.Factory().Create(Controller);
            c.Set(project, removedDiagram);
            Commands.Add(c);
        }

    }

    #region RemovePSMDiagramCommandFactory

    /// <summary>
	/// Factory that creates instances of <see cref="RemovePSMDiagramMacroCommand"/>.
    /// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
    /// therefore all its product Commands recieve reference
    /// to <see cref="ModelController"/> during their initialization.
    /// </summary>
    public class RemovePSMDiagramMacroCommandFactory : ModelCommandFactory<RemovePSMDiagramMacroCommandFactory>
    {
        /// <summary>
        /// Direct constructor is hidden to avoid user making standalone instances. 
        /// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
        /// should be used to get the singleton instance of the factory.
        /// </summary>
        private RemovePSMDiagramMacroCommandFactory()
        {
        }

        /// <summary>
        /// Creates new instance of RemovePSMDiagramMacroCommand
        /// <param name="Controller">Associated model controller</param>
        /// </summary>
        public override IStackedCommand Create(ModelController Controller)
        {
            return new RemovePSMDiagramMacroCommand(Controller);
        }
    }
    #endregion
}
