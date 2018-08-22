using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Removes one or more elements from a diagram, presence of the elements in other diagrams is 
	/// not affected. 
	/// </summary>
	public class DeleteFromDiagramCommand : DiagramCommandBase
	{
		/// <summary>
		/// Elements that should be deleted
		/// </summary>
		[MandatoryArgument]
		public IEnumerable<Element> DeletedElements { get; set; }

        private Dictionary<Element, ViewHelper> DeletedElementsViewHelpers { get; set; }

		public DeleteFromDiagramCommand(DiagramController Controller)
			: base(Controller)
		{
			DeletedElementsViewHelpers = new Dictionary<Element, ViewHelper>();
            Description = CommandDescription.REMOVE_FROM_DIAGRAM;
		}
        
		private bool IsInDiagram(Element element)
		{
			return Diagram.IsElementPresent(element);
		}

		public override bool CanExecute()
		{
			foreach (Element element in DeletedElements)
			{
				if (!IsInDiagram(element))
				{
					ErrorDescription = String.Format(CommandError.CMDERR_DELETE_NONEXISTING, element);
					return false;
				}
			}
			return true;
		}

		internal override void CommandOperation()
		{
			foreach (Element element in DeletedElements)
			{
				DeletedElementsViewHelpers[element] = Diagram.DiagramElements[element];
                Diagram.RemoveModelElement(element);
			}
		}

		internal override OperationResult UndoOperation()
		{
			foreach (Element element in DeletedElements)
			{
                if (IsInDiagram(element))
                {
                    ErrorDescription = CommandError.CMDERR_REMOVED_ELEMENT_BEING_USED;
                    return OperationResult.Failed;
                }
			}
			foreach (Element element in DeletedElements)
            {
                Diagram.AddModelElement(element, DeletedElementsViewHelpers[element]);
            }
            return OperationResult.OK;
		}
	}

	#region DeleteFromDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="DeleteFromDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class DeleteFromDiagramCommandFactory : DiagramCommandFactory<DeleteFromDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private DeleteFromDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of DeleteFromDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new DeleteFromDiagramCommand(diagramController);
		}
	}

	#endregion
}