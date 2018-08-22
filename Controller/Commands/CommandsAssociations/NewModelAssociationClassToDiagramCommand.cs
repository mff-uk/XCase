using System.Collections.Generic;
using XCase.Model;
using XCase.Controller.Commands.Helpers;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Creates new model association class and adds it to a diagram
	/// </summary>
	public class NewModelAssociationClassToDiagramCommand : MacroCommand<DiagramController>
	{
		public NewModelAssociationClassToDiagramCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.ADD_ASSOCIATION_CLASS_MACRO;
		}

		/// <summary>
		/// X coordinate of the new class on the diagram
		/// </summary>
		public double X { get; set; }

		/// <summary>
		/// Y coordinate of the new class on the diagram
		/// </summary>
		public double Y { get; set; }

		NewModelAssociationClassCommand c;

		ElementToDiagramCommand<AssociationClass, AssociationClassViewHelper> d;

		/// <summary>
		/// ViewHelper of the created association 
		/// </summary>
		public AssociationClassViewHelper ViewHelper { get; private set; }

		/// <summary>
		/// New created association class
		/// </summary>
		public AssociationClass CreatedAssociationClass { get; private set; }

		/// <summary>
		/// Name of the created association
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Returns true if element can be added
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
			c = (NewModelAssociationClassCommand)NewModelAssociationClassCommandFactory.Factory().Create(modelController);
			c.AssociatedClasses = associatedClasses;
			c.CreatedAssociationClass = new ElementHolder<AssociationClass>();
			c.Name = Name;

			d = (ElementToDiagramCommand<AssociationClass, AssociationClassViewHelper>)ElementToDiagramCommandFactory<AssociationClass, AssociationClassViewHelper>.Factory().Create(Controller);
			d.IncludedElement = c.CreatedAssociationClass;
			d.ViewHelper.X = X;
			d.ViewHelper.Y = Y;
			Commands.Add(c);
			Commands.Add(d);
		}

		public override void CommandsExecuted()
		{
			base.CommandsExecuted();

			CreatedAssociationClass = c.CreatedAssociationClass.Element;
			ViewHelper = d.ViewHelper;
		}
	}

	#region NewModelAssociationToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelAssociationToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewModelAssociationClassToDiagramCommandFactory : DiagramCommandFactory<NewModelAssociationClassToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelAssociationClassToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelAssociationToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewModelAssociationClassToDiagramCommand(diagramController);
		}
	}

	#endregion
}