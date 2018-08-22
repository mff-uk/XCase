using System;
using System.Diagnostics;
using XCase.UMLController.Commands.Helpers;
using XCase.UMLModel;

namespace XCase.UMLController.Commands
{
	/// <summary>
	/// Adds class present in model into diagram. 
	/// </summary>
	public class AssociationToDiagramCommand : DiagramCommandBase
	{
		//The ElementHolder, through which the reference to the added created class can be passed (for MacroCommands)
		public ElementHolder<Association> IncludedAssociation = null;

		public AssociationToDiagramCommand(DiagramController diagramController)
			: base(diagramController)
		{
		}

		public override bool CanExecute(object parameter)
		{
			// TODO: AssociationToDiagramCommand.CanExecute not implemented properly
			return true;
		}
		
		/// <summary>
		/// ViewHelper of the created association
		/// </summary>
		public AssociationViewHelper ViewHelper { get; private set; }

		public override void CommandOperation(object parameter)
		{
			ViewHelper = new AssociationViewHelper(Diagram, IncludedAssociation.Element);
			Diagram.AddModelElement(IncludedAssociation.Element, ViewHelper);
		}

		public override OperationResult UndoOperation()
		{
			Debug.Assert(IncludedAssociation.HasValue);
			Controller.Diagram.RemoveModelElement(IncludedAssociation.Element);
            return OperationResult.OK;            
		}
	}

	#region AssociationToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="AssociationToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class AssociationToDiagramCommandFactory : DiagramCommandFactory<AssociationToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private AssociationToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of AssociationToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new AssociationToDiagramCommand(diagramController);
		}
	}

	#endregion
}