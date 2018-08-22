using System.Collections.Generic;
using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Creates new association in the model and adds it to the diagram 
	/// </summary>
	public class NewModelAssociationToDiagramCommand : MacroCommand<DiagramController>
	{
		public NewModelAssociationToDiagramCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.ADD_ASSOCIATION_MACRO;
		}

		NewModelAssociationCommand c;

		ElementToDiagramCommand<Association, AssociationViewHelper> d;

		/// <summary>
		/// ViewHelper of the created association 
		/// </summary>
		public AssociationViewHelper ViewHelper { get; private set; }

		/// <summary>
		/// New created association
		/// </summary>
		public Association CreatedAssociation { get; private set; }

		/// <summary>
		/// Name of the created association
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Returns true if Association can be added
		/// </summary>
		public override bool CanExecute()
		{
			return CanExecuteFirst();
		}

		/// <summary>
		/// Perpares this command for execution.
		/// </summary>
		/// <param name="modelController">The ModelController, which will store this command in its undo/redo stacks</param>
		/// <param name="package">The Model Package, in which the class will be created</param>
		/// <param name="associatedClasses">Classes that are associated</param>
		public void Set(ModelController modelController, Package package, IEnumerable<Class> associatedClasses)
		{
			c = (NewModelAssociationCommand) NewModelAssociationCommandFactory.Factory().Create(modelController);
			c.AssociatedClasses = associatedClasses;
			c.CreatedAssociation = new ElementHolder<Association>();
			c.Name = Name;

 			d = (ElementToDiagramCommand<Association, AssociationViewHelper>)ElementToDiagramCommandFactory<Association, AssociationViewHelper>.Factory().Create(Controller);
			d.IncludedElement = c.CreatedAssociation;

			Commands.Add(c);
			Commands.Add(d);
		}

		public override void CommandsExecuted()
		{
			base.CommandsExecuted();

			CreatedAssociation = c.CreatedAssociation.Element;
			ViewHelper = d.ViewHelper;
			AssociatedElements.Add(CreatedAssociation);
		}
	}

	#region NewModelAssociationToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelAssociationToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewModelAssociationToDiagramCommandFactory : DiagramCommandFactory<NewModelAssociationToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelAssociationToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelAssociationToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewModelAssociationToDiagramCommand(diagramController);
		}
	}

	#endregion
}