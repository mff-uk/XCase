using System;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds an element to a diagram
	/// </summary>
	/// <typeparam name="ElementType">type of the added element</typeparam>
	/// <typeparam name="ViewHelperType">type of the added element's ViewHelper</typeparam>
	public class ElementToDiagramCommand<ElementType, ViewHelperType> : DiagramCommandBase
		where ElementType: class, Element
		where ViewHelperType : ViewHelper, new()
	{
		/// <summary>
		/// Element that is added into the diagram
		/// </summary>
		[MandatoryArgument]
		public ElementHolder<ElementType> IncludedElement { get; set; }

		/// <summary>
		/// ViewHelper of the added element
		/// </summary>
		public ViewHelperType ViewHelper { get; private set; }

		/// <summary>
		/// Creates new instance of <see cref="ElementToDiagramCommand{ElementType,ViewHelperType}">ElementToDiagamCommand</see>. 
		/// </summary>
		/// <param name="diagramController"></param>
		public ElementToDiagramCommand(DiagramController diagramController)
			: base(diagramController)
		{
			ConstructorInfo constructor = typeof (ViewHelperType).GetConstructor(new Type[] { typeof (Diagram) });
            Description = CommandDescription.ADD_ELEMENT_TO_DIAGRAM;
			if (constructor == null)
			{
				throw new InvalidOperationException(string.Format("ViewHelperType {0} must have a public constructor with signature 'ViewHelperType(Diagram)' when used in ElementToDiagramCommand.", typeof(ViewHelperType).Name));
			}
			ViewHelper = (ViewHelperType)constructor.Invoke(new object[] { diagramController.Diagram });
		}

		public override bool CanExecute()
		{
			if (IncludedElement.Element != null && Diagram.IsElementPresent(IncludedElement.Element))
			{
				ErrorDescription = string.Format(CommandError.CMDERR_ADDING_PRESENT, IncludedElement.Element);
				return false; 
			}
			return true;
		}

		internal override void CommandOperation()
		{
			Diagram.AddModelElement(IncludedElement.Element, ViewHelper);
			AssociatedElements.Add(IncludedElement.Element);

            if (IncludedElement.Element is PSMElement && Diagram is PSMDiagram)
                (IncludedElement.Element as PSMElement).Diagram = Diagram as PSMDiagram;
		}

		internal override OperationResult UndoOperation()
		{
			Debug.Assert(IncludedElement.HasValue);

            if (Controller.Diagram.IsElementPresent(IncludedElement.Element))
            {
                Controller.Diagram.RemoveModelElement(IncludedElement.Element);
                return OperationResult.OK;
            }
            else
            {
                ErrorDescription = string.Format(CommandError.CMDERR_DELETE_NONEXISTING, IncludedElement.Element);
                return OperationResult.Failed;
            }
		}
	}

	#region ElementToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ElementToDiagramCommand{ElementType,ViewHelperType}"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// <typeparam name="ElementType">Type of the created element</typeparam>
	/// <typeparam name="ViewHelperType">Type of the created element's view helper</typeparam>
	/// </summary>
	public class ElementToDiagramCommandFactory<ElementType, ViewHelperType> : 
		DiagramCommandFactory<ElementToDiagramCommandFactory<ElementType, ViewHelperType>>
		where ElementType : class, Element
		where ViewHelperType : ViewHelper, new()
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ElementToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ElementToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ElementToDiagramCommand<ElementType, ViewHelperType>(diagramController);
		}
	}

	#endregion
}
