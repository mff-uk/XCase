using System;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Shows or hides label on a diagram
	/// </summary>
	public class ShowHideLabelCommand : ViewCommand
	{
		/// <summary>
		/// Creates new instance of <see cref="ShowHideLabelCommand" />. 
		/// </summary>
		/// <param name="Controller">command controller</param>
		public ShowHideLabelCommand(DiagramController Controller)
			: base(Controller)
		{
			Description = CommandDescription.LABEL_SHOWHIDE;
		}

		/// <summary>
		/// Show/Hide action
		/// </summary>
		public enum EShowHide
		{
			Show, 
			Hide
		}
		
		/// <summary>
		/// Command action - show or hide
		/// </summary>
		[MandatoryArgument]
		public EShowHide ? Action
		{
			get; set;
		}

		/// <summary>
		/// ViewHelper of the affected label
		/// </summary>
		[MandatoryArgument]
		AssociationLabelViewHelper LabelViewHelper { get; set; }

		private bool wasVisible;

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			wasVisible = LabelViewHelper.LabelVisible;
			switch (Action)
			{
				case EShowHide.Show:
					LabelViewHelper.LabelVisible = true; 
					break;
				case EShowHide.Hide:
					LabelViewHelper.LabelVisible = false; 
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		internal override OperationResult UndoOperation()
		{
			LabelViewHelper.LabelVisible = wasVisible;
			return OperationResult.OK;
		}
	}

	#region ShowHideLabelCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ShowHideLabelCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ShowHideLabelCommandFactory : DiagramCommandFactory<ShowHideLabelCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ShowHideLabelCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ShowHideLabelCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ShowHideLabelCommand(diagramController);
		}
	}

	#endregion
}