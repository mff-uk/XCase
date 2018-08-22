using System;
using System.Linq;
using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Duplicates diagram in project.
	/// </summary>
	public class DuplicateDiagramCommand: ModelCommandBase
	{
		/// <summary>
		/// Duplicated diagram
		/// </summary>
		[MandatoryArgument]
		public Diagram Diagram { get; set; }

		/// <summary>
		/// Creates new instance of ModelCommandBase.
		/// </summary>
		/// <param name="controller">model controller that will manage command execution</param>
		public DuplicateDiagramCommand(ModelController controller) : base(controller)
		{
			Description = CommandDescription.DUPLICATE_DIAGRAM;
		}

		/// <summary>
		/// Returns true if command can be executed.
		/// </summary>
		/// <returns>True if command can be executed</returns>
		public override bool CanExecute()
		{
			return true; 
		}

		public IEnumerable<Type> PIMElementsOrder { get; set; }

		[CommandResult]
		public Diagram DiagramClone { get; private set; }

		/// <summary>
		/// Executive function of a command
		/// </summary>
		/// <seealso cref="UndoOperation"/>
		internal override void CommandOperation()
		{
			DiagramClone = Diagram.Clone();

			if (Diagram is PIMDiagram)
			{
				Dictionary<Element, ViewHelper> viewHelperCopies = new Dictionary<Element, ViewHelper>();
				
				/* Elements in PIM diagram are loaded in the order of their LoadPriority in registration set */
				foreach (Type ModelElementType in PIMElementsOrder)
				{
					foreach (KeyValuePair<Element, ViewHelper> pair in Diagram.DiagramElements)
					{
						Element element = pair.Key;
						ViewHelper viewHelper = pair.Value;

						if (!viewHelperCopies.ContainsKey(element) && ModelElementType.IsInstanceOfType(element))
						{
							ViewHelper copiedViewHelper = viewHelper.CreateCopy(DiagramClone, null);
							viewHelperCopies.Add(element, copiedViewHelper);
						}
					}
				}

				Diagram.FillCopy(DiagramClone, Controller.Model, null, viewHelperCopies);
			}
			else
			{
				IList<Element> ordered;
				
				// order 
				PSMTree.ReturnElementsInPSMOrder(((PSMDiagram)Diagram).Roots.Cast<Element>(), out ordered, true);

				// clone the selection 
                ElementCopiesMap createdCopies = new ElementCopiesMap();
				foreach (Element element in ordered)
				{
					Element copy = element.CreateCopy(Controller.Model, createdCopies);
					createdCopies[element] = copy;
				}
				// representants must be handled separately after all copies are created 
				PSMTree.CopyRepresentantsRelations(createdCopies);

				// clone viewhelpers
				Dictionary<Element, ViewHelper> createdViewHelpers = new Dictionary<Element, ViewHelper>();

				foreach (Element element in ordered)
				{
					ViewHelper viewHelper = Diagram.DiagramElements[element];
					ViewHelper copiedViewHelper = viewHelper.CreateCopy(DiagramClone, createdCopies);

					createdViewHelpers.Add(element, copiedViewHelper);
				}

				Diagram.FillCopy(DiagramClone, Controller.Model, createdCopies, createdViewHelpers);
			}

			DiagramClone.Caption += " (copy)";
			Controller.Project.AddDiagram(DiagramClone);
		}

		/// <summary>
		/// Undo executive function of a command. Should revert the <see cref="CommandOperation"/> executive 
		/// function and return the state to the state before CommandOperation was execute.
		/// <returns>returns <see cref="CommandBase.OperationResult.OK"/> if operation succeeded, <see cref="CommandBase.OperationResult.Failed"/> otherwise</returns>
		/// </summary>
		/// <remarks>
		/// <para>If  <see cref="CommandBase.OperationResult.Failed"/> is returned, whole undo stack is invalidated</para>
		/// </remarks>
		internal override OperationResult UndoOperation()
		{
			if (Controller.Project.Diagrams.Contains(DiagramClone))
			{
				Controller.Project.RemoveDiagram(DiagramClone);
				return OperationResult.OK;
			}
			else
			{
				ErrorDescription = String.Format(CommandError.CMDERR_DIAGRAM_NOT_IN_PROJECT, DiagramClone.Caption);
				return OperationResult.Failed;
			}
		}

		internal override void RedoOperation()
		{
			if (!Controller.Project.Diagrams.Contains(DiagramClone))
			{	
				Controller.Project.AddDiagram(DiagramClone);
			}
		}
	}

	#region DuplicateDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="DuplicateDiagramCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class DuplicateDiagramCommandFactory : ModelCommandFactory<DuplicateDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private DuplicateDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of DuplicateDiagramCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new DuplicateDiagramCommand(modelController);
		}
	}

	#endregion
}