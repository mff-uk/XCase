using System;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// This command removes association from the diagram and the whole subtree under the 
	/// association is added as a new root subtree. 
	/// </summary>
	public class CutAssociationToRootCommand : DiagramCommandBase
	{
		/// <summary>
		/// Association that will be removed
		/// </summary>
		[MandatoryArgument]
		public PSMAssociation PSMAssociation { get; set; }

		private ViewHelper associationViewHelper;

		/// <summary>
		/// Creates new instance of <see cref="CutAssociationToRootCommand">CutAssociationToRootCommand</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public CutAssociationToRootCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_CUT_ASSOCIATION_TO_ROOT;
		}

		public override bool CanExecute()
		{
			return (PSMAssociation.Child is PSMClass);
		}

		internal override void CommandOperation()
		{
			associationViewHelper = Diagram.DiagramElements[PSMAssociation];
			Diagram.RemoveModelElement(PSMAssociation);
			PSMAssociation.RemoveMeFromModel();
			(Diagram as PSMDiagram).Roots.Add(PSMAssociation.Child as PSMClass);
			AssociatedElements.Add(PSMAssociation.Child);
		}

		internal override OperationResult UndoOperation()
		{
			(Diagram as PSMDiagram).Roots.Remove(PSMAssociation.Child as PSMClass);
			PSMAssociation.PutMeBackToModel();
			Diagram.AddModelElement(PSMAssociation, associationViewHelper);
			return OperationResult.OK;
		}
	}

	#region CutAssociationToRootCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="CutAssociationToRootCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class CutAssociationToRootCommandFactory : DiagramCommandFactory<CutAssociationToRootCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private CutAssociationToRootCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of CutAssociationToRootCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new CutAssociationToRootCommand(diagramController);
		}
	}

	#endregion
}
