using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Moves element on a diagram
	/// </summary>
	public class MoveElementCommand: ViewCommand
	{
		/// <summary>
		/// New x coordinate of an element
		/// </summary>
		public double ? X { get; set; }
        
		/// <summary>
		/// New y coordinate of an element
		/// </summary>
		public double ? Y { get; set; }

		private double oldX;

		private double oldY;

		/// <summary>
		/// Moved element's ViewHelper
		/// </summary>
		[MandatoryArgument]
		public PositionableElementViewHelper ViewHelper { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="MoveElementCommand" />. 
		/// </summary>
		/// <param name="diagramController">command controller</param>
		public MoveElementCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.MOVE_ELEMENT;
		}

		public override bool CanExecute()
		{
			return true; 
		}

		internal override void CommandOperation()
		{
			if (X.HasValue)
			{
				oldX = ViewHelper.X;
				ViewHelper.X = X.Value;
			}

			if (Y.HasValue)
			{
				oldY = ViewHelper.Y;
				ViewHelper.Y = Y.Value;
			}
		}

		internal override OperationResult UndoOperation()
		{
			if (Y.HasValue)
			{
				ViewHelper.Y = oldY;
			}
			if (X.HasValue)
			{
				ViewHelper.X = oldX;
			}
			return OperationResult.OK;
		}
	}

	#region MoveElementCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="MoveElementCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class MoveElementCommandFactory : DiagramCommandFactory<MoveElementCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private MoveElementCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of MoveElementCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new MoveElementCommand(diagramController);
		}
	}

	#endregion
}