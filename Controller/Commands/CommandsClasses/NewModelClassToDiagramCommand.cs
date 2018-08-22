using System;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Adds a new class to the Model and then adds this class 
    /// to the Diagram specified by the DiagramController.
	/// Joint <see cref="NewModelClassCommand"/> and <see cref="ElementToDiagramCommand{ElementType,ViewHelperType}"/>
    /// </summary>
    /// 
    public class NewModelClassToDiagramCommand : NewPositionableElementMacroCommand
    {
    	public string ClassName { get; set; }
    	
    	/// <summary>
        /// Constructs this command.
        /// </summary>
        /// <param name="diagramController">Specifies the Diagram, to which the class will be added</param>
        public NewModelClassToDiagramCommand(DiagramController diagramController)
            : base(diagramController)
        {
            Description = CommandDescription.ADD_CLASS_MACRO;
        }

		[CommandResult]
		public ElementHolder<PIMClass> CreatedClass { get; private set; }


		NewModelClassCommand c;
		ElementToDiagramCommand<PIMClass, ClassViewHelper> d;

    	/// <summary>
        /// Perpares this command for execution.
        /// </summary>
        /// <param name="modelController">The ModelController, which will store this command in its undo/redo stacks</param>
        /// <param name="package">The Model Package, in which the class will be created</param>
        public override void Set(ModelController modelController, Package package)
        {            
			if (CreatedClass == null)
				CreatedClass = new ElementHolder<PIMClass>();
    		c = (NewModelClassCommand) NewModelClassCommandFactory.Factory().Create(modelController);
    		c.ClassName = ClassName;
        	c.Package = package;
    		c.CreatedClass = CreatedClass;

			d = (ElementToDiagramCommand<PIMClass, ClassViewHelper>)ElementToDiagramCommandFactory<PIMClass, ClassViewHelper>.Factory().Create(Controller);
    		d.IncludedElement = CreatedClass;
            d.ViewHelper.X = X;
			d.ViewHelper.Y = Y;
    		
            Commands.Add(c);
            Commands.Add(d);
        }

		public override void CommandsExecuted()
		{
			base.CommandsExecuted();
			this.ViewHelper = d.ViewHelper;
			this.CreatedClass = c.CreatedClass;
		}
    }

	#region NewModelClassToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelClassToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewModelClassToDiagramCommandFactory : DiagramCommandFactory<NewModelClassToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelClassToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelClassToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewModelClassToDiagramCommand(diagramController);
		}
	}

	#endregion
}
