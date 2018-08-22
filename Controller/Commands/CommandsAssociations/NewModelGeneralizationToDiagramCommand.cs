using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds new generalization to the model and includes the new generalization to the current diagram
	/// </summary>
	public class NewModelGeneralizationToDiagramCommand: MacroCommand<DiagramController>
	{
		public NewModelGeneralizationToDiagramCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.ADD_GENERALIZATION_MACRO;
		}

		NewModelGeneralizationCommand c;

		ElementToDiagramCommand<Generalization, GeneralizationViewHelper> d;

		/// <summary>
		/// ViewHelper of the created Generalization 
		/// </summary>
		public GeneralizationViewHelper ViewHelper { get; private set; }

		/// <summary>
		/// New craeted generalization
		/// </summary>
		public Generalization CreatedGeneralization { get; private set; }

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
		/// <param name="general">general (parent) class</param>
		/// <param name="specific">specific (child) class</param>
		public void Set(ModelController modelController, Package package, Class general, Class specific)
		{
			c = (NewModelGeneralizationCommand) NewModelGeneralizationCommandFactory.Factory().Create(modelController);
			c.General = general;
			c.Specific = specific;
			c.CreatedGeneralization = new ElementHolder<Generalization>();

			d = (ElementToDiagramCommand<Generalization, GeneralizationViewHelper>)ElementToDiagramCommandFactory<Generalization, GeneralizationViewHelper>.Factory().Create(Controller);
			d.IncludedElement = c.CreatedGeneralization;

			Commands.Add(c);
			Commands.Add(d);
		}

		public override void CommandsExecuted()
		{
			base.CommandsExecuted();

			CreatedGeneralization = c.CreatedGeneralization.Element;
			ViewHelper = d.ViewHelper;
		}		
	}

	#region NewModelGeneralizationToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelGeneralizationToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewModelGeneralizationToDiagramCommandFactory : DiagramCommandFactory<NewModelGeneralizationToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelGeneralizationToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelGeneralizationToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewModelGeneralizationToDiagramCommand(diagramController);
		}
	}

	#endregion
}