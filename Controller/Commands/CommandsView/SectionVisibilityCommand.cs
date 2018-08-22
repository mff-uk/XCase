using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Manages visibility of Class sections
	/// </summary>
    public class SectionVisibilityCommand : ViewCommand
	{
		/// <summary>
		/// Action changing visibiity of a section 
		/// </summary>
		public enum ESectionVisibilityAction
		{
			CollapseAttributes,
			ShowAttributes,
			CollapseOperations,
			ShowOperations,
            CollapseElementNameLabel,
            ShowElementNameLabel
		}

		[MandatoryArgument]
		public ESectionVisibilityAction? Action { get; set; }

		[MandatoryArgument]
		public ClassViewHelper ViewHelper { get; set; }

		public SectionVisibilityCommand(DiagramController Controller)
			: base(Controller)
		{
		}

		public override bool CanExecute()
		{
			return true;
		}

		private bool oldAttributesState;

		private bool oldOperationsState;

        private bool oldElementLabelState;

		internal override void CommandOperation()
		{
			oldAttributesState = ViewHelper.AttributesCollapsed;
			oldOperationsState = ViewHelper.OperationsCollapsed;
            oldElementLabelState = ViewHelper.ElementNameLabelCollapsed;
			switch (Action.Value)
			{
				case ESectionVisibilityAction.CollapseAttributes:
					ViewHelper.AttributesCollapsed = true;
					break;
				case ESectionVisibilityAction.ShowAttributes:
					ViewHelper.AttributesCollapsed = false;
					break;
				case ESectionVisibilityAction.CollapseOperations:
					ViewHelper.OperationsCollapsed = true;
					break;
				case ESectionVisibilityAction.ShowOperations:
					ViewHelper.OperationsCollapsed = false;
					break;
                case ESectionVisibilityAction.CollapseElementNameLabel:
                    ViewHelper.ElementNameLabelCollapsed = true;
                    break;
                case ESectionVisibilityAction.ShowElementNameLabel:
                    ViewHelper.ElementNameLabelCollapsed = false;
                    break;
            }
		}

		internal override OperationResult UndoOperation()
		{
			switch (Action.Value)
			{
				case ESectionVisibilityAction.CollapseAttributes:
					ViewHelper.AttributesCollapsed = oldAttributesState;
					break;
				case ESectionVisibilityAction.ShowAttributes:
					ViewHelper.AttributesCollapsed = oldAttributesState;
					break;
				case ESectionVisibilityAction.CollapseOperations:
					ViewHelper.OperationsCollapsed = oldOperationsState;
					break;
				case ESectionVisibilityAction.ShowOperations:
					ViewHelper.OperationsCollapsed = oldOperationsState;
					break;
                case ESectionVisibilityAction.CollapseElementNameLabel:
                    ViewHelper.ElementNameLabelCollapsed = oldElementLabelState;
                    break;
                case ESectionVisibilityAction.ShowElementNameLabel:
                    ViewHelper.ElementNameLabelCollapsed = oldElementLabelState;
                    break;
            }
			return OperationResult.OK;
		}
	}

	#region SectionVisibilityCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="SectionVisibilityCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreteCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class SectionVisibilityCommandFactory : DiagramCommandFactory<SectionVisibilityCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private SectionVisibilityCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of SectionVisibilityCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new SectionVisibilityCommand(diagramController);
		}
	}

	#endregion
}