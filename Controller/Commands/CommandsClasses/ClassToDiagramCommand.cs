using System;
using System.Diagnostics;
using XCase.UMLController.Commands.Helpers;
using XCase.UMLModel;

namespace XCase.UMLController.Commands
{
    /// <summary>
    /// Adds class present in model into diagram. 
    /// Reference to the class should be passed as <see cref="CommandBase.Execute(object)"/> parameter; 
    /// </summary>
    public class ClassToDiagramCommand: NewPositionableElementCommand
    {
		[MandatoryArgument]
		public ElementHolder<Class> IncludedClass { get; set; }

        public ClassToDiagramCommand(DiagramController diagramController)
            : base(diagramController)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return IncludedClass.HasValue;
        }
        
        public override void CommandOperation(object parameter)
        {
			ViewHelper= new ClassViewHelper(Diagram) { X = X, Y = Y, Height = Height, Width = Width };
            Diagram.AddModelElement(IncludedClass.Element, ViewHelper);
        }

        public override OperationResult UndoOperation()
        {
            Debug.Assert(IncludedClass.HasValue);

            if (Controller.Diagram.DiagramElements.ContainsKey(IncludedClass.Element))
            {
                Controller.Diagram.RemoveModelElement(IncludedClass.Element);
                return OperationResult.OK;
            }
            else return OperationResult.Failed;
        }
    }

	#region ClassToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ClassToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ClassToDiagramCommandFactory : DiagramCommandFactory<ClassToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ClassToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ClassToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ClassToDiagramCommand(diagramController);
		}
	}

	#endregion
}
