using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Includes elements to a diagram
	/// </summary>
	public class IncludeElementsCommand : DiagramCommandBase
	{

		public Dictionary<Element, ViewHelper> IncludedElements { get; private set; }

		/// <summary>
		/// Creates new instance of <see cref="IncludeElementsCommand">ElementToDiagamCommand</see>. 
		/// </summary>
		/// <param name="diagramController"></param>
		public IncludeElementsCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.INCLUDE_ELEMENTS;
			IncludedElements = new Dictionary<Element, ViewHelper>();
		}

		public override bool CanExecute()
		{
			foreach (Element element in IncludedElements.Keys)
			{
				if (Controller.Diagram.IsElementPresent(element))
				{
					ErrorDescription = String.Format(CommandError.CMDERR_ADDING_PRESENT, element);
					return false; 
				}
			}
			return true;
		}

		internal override void CommandOperation()
		{
			foreach (KeyValuePair<Element, ViewHelper> pair in IncludedElements)
			{
				Diagram.AddModelElement(pair.Key, pair.Value);	
				AssociatedElements.Add(pair.Key);
			}
		}

		internal override OperationResult UndoOperation()
		{
			foreach (Element element in IncludedElements.Keys)
			{
                if (Controller.Diagram.IsElementPresent(element))
                {
                    Controller.Diagram.RemoveModelElement(element);
                }
                else
                {
                    ErrorDescription = string.Format(CommandError.CMDERR_DELETE_NONEXISTING, element);
                    return OperationResult.Failed;
                }
			}
			return OperationResult.OK;
		}
	}

	#region IncludeElementsCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="IncludeElementsCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class IncludeElementsCommandFactory : DiagramCommandFactory<IncludeElementsCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private IncludeElementsCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of IncludeElementsCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new IncludeElementsCommand(diagramController);
		}
	}

	#endregion
}
