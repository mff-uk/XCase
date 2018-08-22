using System;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Resizes element on a  diagram
	/// </summary>
	public class ResizeElementCommand : ViewCommand
	{
		/// <summary>
		/// Creates new instance of <see cref="ResizeElementCommand" />.	
		/// </summary>
		/// <param name="Controller"></param>
		public ResizeElementCommand(DiagramController Controller)
			: base(Controller)
		{
			Description = CommandDescription.RESIZE_ELEMENT;
		}

		/// <summary>
		/// New width of the resized element
		/// </summary>
		public double Width { get; set; }

		/// <summary>
		/// New height of the resized element
		/// </summary>
		public double Height { get; set; }

		/// <summary>
		/// Resized element's ViewHelper
		/// </summary>
		[MandatoryArgument]
		public PositionableElementViewHelper ViewHelper { get; set; }

		private double oldWidth;

		private double oldHeight;

		public override bool CanExecute()
		{
			return true;
		}

		internal override void CommandOperation()
		{
			oldWidth = ViewHelper.Width;
			oldHeight = ViewHelper.Height;

			ViewHelper.Width = Width;
			ViewHelper.Height = Height;
		}

		internal override OperationResult UndoOperation()
		{
			ViewHelper.Width = oldWidth;
			ViewHelper.Height = oldHeight;
			return OperationResult.OK;
		}
	}

	#region ResizeElementCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="ResizeElementCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class ResizeElementCommandFactory : DiagramCommandFactory<ResizeElementCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private ResizeElementCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of ResizeElementCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new ResizeElementCommand(diagramController);
		}
	}

	#endregion
}