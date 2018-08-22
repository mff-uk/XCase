using System.Collections.Generic;
using System.Linq;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Joins given PSM associations to a single one starting at the Parent
    /// component of the joined associations and ending in a new Class Union component.
    /// The new association inherits all the nesting joins of the joined associations that
    /// are removed after the class union creation.
    /// </summary>
    public class JoinAssociationsToClassUnionMacroCommand : MacroCommand<DiagramController>
    {
        /// <summary>
        /// Gets or sets the holder for the PSM component that is the root of all
        /// the joined associations.
        /// </summary>
        private Helpers.ElementHolder<PSMSuperordinateComponent> ParentHolder { get; set; }
        
        /// <summary>
        /// Gets or sets the list of PSM Associations to be joined into
        /// one PSM Class Union.
        /// </summary>
		private List<PSMAssociation> JoinedAssociations { get; set; }

        /// <summary>
        /// Gets or sets the holder for the created PSM Association.
        /// </summary>
        private Helpers.ElementHolder<PSMAssociation> CreatedAssociationHolder { get; set; }

        /// <summary>
        /// Gets or sets the holder for the created class union viewed as an association child.
        /// </summary>
        private Helpers.ElementHolder<PSMAssociationChild> CreatedAssocChild { get; set; }
        
        /// <summary>
        /// Gets or sets the holder for the created class union.
        /// </summary>
        [CommandResult]
        public Helpers.ElementHolder<PSMClassUnion> CreatedUnion { get; set; }

        /// <summary>
        /// Creates a new command instance.
        /// </summary>
        /// <param name="controller">Associatied diagram controller</param>
        public JoinAssociationsToClassUnionMacroCommand(DiagramController controller)
            : base(controller)
        {
        }

        public override bool CanExecute()
        {
            bool can = ParentHolder != null && ParentHolder.Element != null &&  JoinedAssociations != null && JoinedAssociations.Count > 0;
            if (!can)
                return false;
            
            foreach (PSMAssociation assoc in JoinedAssociations)
            {
                if (assoc.Parent != ParentHolder.Element)
                {
                    can = false;
                    break;
                }
            }

            return can;
        }

        /// <summary>
        /// Sets this command for use.
        /// </summary>
        /// <param name="parent">Reference to the PSM component that is the root of joined associations</param>
        /// <param name="joinedAssociations">List of associations to be joined to one class union</param>
        public void Set(PSMSuperordinateComponent parent, IEnumerable<PSMAssociation> joinedAssociations)
        {
			JoinedAssociations = new List<PSMAssociation>(joinedAssociations);
            
            PSMSubordinateComponent first = parent.Components.First(assoc => JoinedAssociations.Contains(assoc as PSMAssociation));
            int? index = parent.Components.IndexOf(first);


            if (ParentHolder == null)
                ParentHolder = new Helpers.ElementHolder<PSMSuperordinateComponent>();
            ParentHolder.Element = parent;

            if (CreatedUnion == null)
                CreatedUnion = new Helpers.ElementHolder<PSMClassUnion>();

            if (CreatedAssocChild == null)
                CreatedAssocChild = new Helpers.ElementHolder<PSMAssociationChild>();

            if (CreatedAssociationHolder == null)
                CreatedAssociationHolder = new Helpers.ElementHolder<PSMAssociation>();

            NewPSMClassUnionCommand c1 = NewPSMClassUnionCommandFactory.Factory().Create(Controller) as NewPSMClassUnionCommand;
            c1.CreatedUnion = CreatedUnion;
        	c1.Parent = parent;
            Commands.Add(c1);

            Helpers.HolderConvertorCommand<PSMClassUnion, PSMAssociationChild> cc = 
                new Commands.Helpers.HolderConvertorCommand<PSMClassUnion, PSMAssociationChild>(CreatedUnion, 
                    CreatedAssocChild);
            Commands.Add(cc);

            NewPSMAssociationCommand newAssocCommand = NewPSMAssociationCommandFactory.Factory().Create(Controller.ModelController) as NewPSMAssociationCommand;
            newAssocCommand.Set(ParentHolder, CreatedAssocChild, CreatedAssociationHolder, index);
            Commands.Add(newAssocCommand);

            CopyNestingJoinsCommand copyNJcommand = CopyNestingJoinsCommandFactory.Factory().Create(Controller.ModelController) as CopyNestingJoinsCommand;
            copyNJcommand.Set(CreatedAssociationHolder, JoinedAssociations);
            Commands.Add(copyNJcommand);

            /*GetClassUnionContentCommand c4 = GetClassUnionContentCommandFactory.Factory().Create(Controller.ModelController) as GetClassUnionContentCommand;
            c4.Set(joinedAssociations, CreatedUnion);
            Commands.Add(c4);*/

            PutClassesToUnionCommand putCommand = PutClassesToUnionCommandFactory.Factory().Create(Controller.ModelController) as PutClassesToUnionCommand;
            putCommand.Set(joinedAssociations, CreatedUnion);
            Commands.Add(putCommand);

            DeleteFromPSMDiagramCommand delCommand = DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller) as DeleteFromPSMDiagramCommand;
            delCommand.DeletedElements = new List<Element>(JoinedAssociations.Cast<Element>());
            delCommand.CheckOrdering = false;
            Commands.Add(delCommand);

            foreach (PSMAssociation assoc in joinedAssociations)
            {
                PSMClassUnion union = assoc.Child as PSMClassUnion;
                if (union != null)
                {
                    MoveClassUnionContentCommand moveCommand = MoveClassUnionContentCommandFactory.Factory().Create(Controller) as MoveClassUnionContentCommand;
                    moveCommand.Set(union, CreatedUnion);
                    Commands.Add(moveCommand);
                    DeleteFromPSMDiagramCommand delUnion = DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller) as DeleteFromPSMDiagramCommand;
                    delUnion.DeletedElements = new List<Element>();
                    delUnion.DeletedElements.Add(union);
                    Commands.Add(delUnion);
                }
            }

        	ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper> includeAssociation = (ElementToDiagramCommand<PSMAssociation, PSMAssociationViewHelper>)ElementToDiagramCommandFactory<PSMAssociation, PSMAssociationViewHelper>.Factory().Create(Controller);
			includeAssociation.IncludedElement = CreatedAssociationHolder;
			Commands.Add(includeAssociation);
        }

		public override void CommandsExecuted()
		{
			base.CommandsExecuted();
			AssociatedElements.Add(CreatedUnion.Element);
		}
    }

	#region JoinAssociationsToClassUnionMacroCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="JoinAssociationsToClassUnionMacroCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class JoinAssociationsToClassUnionMacroCommandFactory : DiagramCommandFactory<JoinAssociationsToClassUnionMacroCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private JoinAssociationsToClassUnionMacroCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of JoinAssociationsToClassUnionMacroCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new JoinAssociationsToClassUnionMacroCommand(diagramController);
		}
	}

	#endregion
}
