using System;
using System.Collections.Generic;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Switches 
	/// </summary>
	public class SwitchAssociationDiamondCommand: ViewCommand
	{
		private bool oldValue;

		[MandatoryArgument]
		public Association Association { get; set; }

		[MandatoryArgument]
		public AssociationViewHelper AssociationViewHelper { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="ViewCommand" />. 
		/// </summary>
		/// <param name="diagramController">command's controller</param>
		public SwitchAssociationDiamondCommand(DiagramController diagramController) : 
			base(diagramController)
		{
			Description = CommandDescription.SWITCH_DIAMOND;
		}

		/// <summary>
		/// Returns true if command can be executed.
		/// </summary>
		/// <returns>True if command can be executed</returns>
		public override bool CanExecute()
		{
			if ((AssociationViewHelper.UseDiamond && AssociationViewHelper.AssociationEndsViewHelpers.Count <= 2) ||
				!AssociationViewHelper.UseDiamond)
				return true;
			else
			{
				ErrorDescription = CommandError.DIAMOND_REQUIRED;
				return false;
			}
		}


		private Dictionary<int, List<rPoint>> oldPoints;

		/// <summary>
		/// Executive function of a command
		/// </summary>
		/// <seealso cref="UndoOperation"/>
		internal override void CommandOperation()
		{
			oldValue = AssociationViewHelper.UseDiamond;
			Diagram.RemoveModelElement(Association);
			
			oldPoints = new Dictionary<int, List<rPoint>>();

			int i = 0;
			foreach (AssociationEndViewHelper endViewHelper in AssociationViewHelper.AssociationEndsViewHelpers)
			{
				oldPoints[i++] = new List<rPoint>(endViewHelper.Points);
				endViewHelper.Points.Clear();
			}

			if (!AssociationViewHelper.UseDiamond)
			{
				AssociationViewHelper.X = 0;
				AssociationViewHelper.Y = 0;
			}

			AssociationViewHelper.UseDiamond = !AssociationViewHelper.UseDiamond;
			Diagram.AddModelElement(Association, AssociationViewHelper);

			AssociatedElements.Add(Association);
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
			Diagram.RemoveModelElement(Association);

			int i = 0;
			foreach (AssociationEndViewHelper endViewHelper in AssociationViewHelper.AssociationEndsViewHelpers)
			{
				endViewHelper.Points.Clear();
				endViewHelper.Points.AppendRange(oldPoints[i++]);
				
			}

			AssociationViewHelper.UseDiamond = oldValue;
			Diagram.AddModelElement(Association, AssociationViewHelper);

			return OperationResult.OK;
		}
	}

	#region SwitchAssociationDiamondCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="SwitchAssociationDiamondCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class SwitchAssociationDiamondCommandFactory : DiagramCommandFactory<SwitchAssociationDiamondCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private SwitchAssociationDiamondCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of SwitchAssociationDiamondCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new SwitchAssociationDiamondCommand(diagramController);
		}
	}

	#endregion
}