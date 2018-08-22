using System;
using System.Diagnostics;
using XCase.UMLController.Commands.Helpers;
using XCase.UMLModel;

namespace XCase.UMLController.Commands
{
    public class CommentToDiagramCommand: NewPositionableElementCommand
    {
        //The IncludedComment, through which the reference to the added created class can be passed (for MacroCommands)
		[MandatoryArgument]
		public ElementHolder<Comment> IncludedComment { get; set; }

        public CommentToDiagramCommand(DiagramController diagramController)
            : base(diagramController)
        {
        }

        public override bool CanExecute(object parameter)
        {
            return IncludedComment.HasValue;
        }

        public override void CommandOperation(object parameter)
        {
			ViewHelper = new CommentViewHelper(Diagram) { X = X, Y = Y, Height = Height, Width = Width };
			Diagram.AddModelElement(IncludedComment.Element, ViewHelper);
        }

        public override OperationResult UndoOperation()
        {
			Debug.Assert(IncludedComment.HasValue);
            
            if (Controller.Diagram.DiagramElements.ContainsKey(IncludedComment.Element))
            {
                Controller.Diagram.RemoveModelElement(IncludedComment.Element);
                return OperationResult.OK;
            }
            else return OperationResult.Failed;
        }
    }

	#region CommentToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="CommentToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class CommentToDiagramCommandFactory : DiagramCommandFactory<CommentToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private CommentToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of CommentToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new CommentToDiagramCommand(diagramController);
		}
	}

	#endregion
}
