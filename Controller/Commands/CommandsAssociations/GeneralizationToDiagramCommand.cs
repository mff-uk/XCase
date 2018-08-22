using System;
using XCase.UMLController.Commands.Helpers;
using XCase.UMLModel;

namespace XCase.UMLController.Commands
{
	public class GeneralizationToDiagramCommand : DiagramCommandBase
	{
		//The ElementHolder, through which the reference to the added created class can be passed (for MacroCommands)
		public ElementHolder<Generalization> IncludedGeneralization = null;

		/// <summary>
		/// ViewHelper of the created generalization
		/// </summary>
		public GeneralizationViewHelper ViewHelper { get; private set; }

		public GeneralizationToDiagramCommand(DiagramController Controller)
			: base(Controller)
		{
		}

		public override bool CanExecute(object parameter)
		{
			// TODO: GeneralizationToDiagramCommand.CanExecute not implemented properly
			return true;
		}

		public override void CommandOperation(object parameter)
		{
			ViewHelper = new GeneralizationViewHelper(Diagram, IncludedGeneralization.Element);
			Diagram.AddModelElement(IncludedGeneralization.Element, ViewHelper);
		}

		public override OperationResult UndoOperation()
		{
            Controller.Diagram.RemoveModelElement(IncludedGeneralization.Element);
            return OperationResult.OK;
        }
	}

	#region GeneralizationToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="GeneralizationToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class GeneralizationToDiagramCommandFactory : DiagramCommandFactory<GeneralizationToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private GeneralizationToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of GeneralizationToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new GeneralizationToDiagramCommand(diagramController);
		}
	}

	#endregion
}