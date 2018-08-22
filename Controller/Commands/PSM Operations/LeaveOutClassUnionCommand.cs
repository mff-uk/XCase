using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using XCase.Controller;
using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds an element to a diagram
	/// </summary>
	public class LeaveOutClassUnionCommand : DiagramCommandBase
	{
		/// <summary>
		/// Creates new instance of <see cref="LeaveOutClassUnionCommand">LeaveOutClassUnion</see>. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public LeaveOutClassUnionCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.PSM_LEAVE_OUT_CLASS_UNION;
		}

        /// <summary>
        /// Collection of the union components that were moved to diagram roots.
        /// </summary>
        private PSMAssociationChild[] movedChilds;

        /// <summary>
        /// Command used to delete the parent association of the union.
        /// </summary>
        private DeleteFromPSMDiagramCommand delCmd;

        [MandatoryArgument]
		public PSMClassUnion UnionLeftOut
		{
			get;
			set;
		}

		public override bool CanExecute()
		{
			return (UnionLeftOut != null);
		}

		internal override void CommandOperation()
		{
            movedChilds = new PSMAssociationChild[UnionLeftOut.Components.Count];
            UnionLeftOut.Components.CopyTo(movedChilds, 0);

            foreach (PSMAssociationChild child in movedChilds)
            {
                UnionLeftOut.Components.Remove(child);
                ((PSMDiagram)Diagram).Roots.Add((PSMClass)child);
            }

            delCmd = DeleteFromPSMDiagramCommandFactory.Factory().Create(Controller) as DeleteFromPSMDiagramCommand;
            delCmd.DeletedElements = new List<Element>();
            delCmd.DeletedElements.Add(UnionLeftOut.ParentAssociation);
            delCmd.DeletedElements.Add(UnionLeftOut);
            delCmd.CommandOperation();
		}

		internal override OperationResult UndoOperation()
		{
            delCmd.UndoOperation();

            foreach (PSMAssociationChild child in movedChilds)
            {
                ((PSMDiagram)Diagram).Roots.Remove((PSMClass)child);
                UnionLeftOut.Components.Add(child);
            }

			return OperationResult.OK;
		}

        internal override void RedoOperation()
        {
            foreach (PSMAssociationChild child in movedChilds)
            {
                UnionLeftOut.Components.Remove(child);
                ((PSMDiagram)Diagram).Roots.Add((PSMClass)child);
            }

            delCmd.RedoOperation();
        }
	}

	#region LeaveOutClassUnionCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="LeaveOutClassUnionCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class LeaveOutClassUnionCommandFactory : DiagramCommandFactory<LeaveOutClassUnionCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private LeaveOutClassUnionCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of LeaveOutClassUnionCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new LeaveOutClassUnionCommand(diagramController);
		}
	}

	#endregion
}
