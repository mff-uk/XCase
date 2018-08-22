using System;
using System.Diagnostics;
using XCase.UMLController.Commands.Helpers;
using XCase.UMLModel;

namespace XCase.UMLController.Commands
{
	public class AssociationClassToDiagramCommand: DiagramCommandBase
	{
		//The ElementHolder, through which the reference to the added created class can be passed (for MacroCommands)
		public ElementHolder<AssociationClass> IncludedAssociationClass = null;

		public AssociationClassToDiagramCommand(DiagramController diagramController)
			: base(diagramController)
		{
		}

		public override bool CanExecute(object parameter)
		{
			// TODO: AssociationClassToDiagramCommand.CanExecute not implemented properly
			return true;
		}
		
		/// <summary>
		/// ViewHelper of the created AssociationClass
		/// </summary>
		public AssociationClassViewHelper ViewHelper { get; private set; }

		public double X { get; set; }

		public double Y { get; set; }

		public override void CommandOperation(object parameter)
		{
			ViewHelper = new AssociationClassViewHelper(Diagram, IncludedAssociationClass.Element) { X = X, Y = Y };
			Diagram.AddModelElement(IncludedAssociationClass.Element, ViewHelper);
		}

		public override OperationResult UndoOperation()
		{
			Debug.Assert(IncludedAssociationClass.HasValue);

			// TODO: removing AssociationClass from diagram
			//DiagramController.Diagram.Remove(includedAssociationClass); 
			throw new NotImplementedException("Method or operation is not implemented");
            
		}
	}

	#region AssociationClassToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="AssociationClassToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class AssociationClassToDiagramCommandFactory : DiagramCommandFactory<AssociationClassToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private AssociationClassToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of AssociationClassToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new AssociationClassToDiagramCommand(diagramController);
		}
	}

	#endregion
}