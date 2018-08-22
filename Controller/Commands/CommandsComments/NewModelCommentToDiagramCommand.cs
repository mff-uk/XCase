using XCase.Controller.Commands.Helpers;
using XCase.Model;
using System;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Creates new comment in the model and adds it to the diagram
	/// </summary>
	public class NewModelCommentToDiagramCommand : NewPositionableElementMacroCommand
	{
		/// <summary>
		/// Element that the commmentary annotates. Field is optional and commentary can be placed
		/// freely on the diagram without being connected to a specific element.
		/// </summary>
		public Element AnnotatedElement { get; set; }

		/// <summary>
		/// Comment text
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Creates new instance of <see cref="NewModelCommentToDiagramCommand" />. 
		/// </summary>
		/// <param name="diagramController">controller of the command</param>
		public NewModelCommentToDiagramCommand(DiagramController diagramController)
			: base(diagramController)
		{
			Description = CommandDescription.ADD_COMMENT_MACRO;
		}

		NewModelCommentCommand c;
		ElementToDiagramCommand<Comment, CommentViewHelper> d;

		public override void Set(ModelController modelController, Package package)
		{
			//The one used to carry the reference to the modelclass from one command to another

			
			c = (NewModelCommentCommand)NewModelCommentCommandFactory.Factory().Create(modelController);
			c.CreatedComment = new ElementHolder<Comment>();
			c.AnnotatedElement = AnnotatedElement;
			c.Text = Text;

			d = (ElementToDiagramCommand<Comment, CommentViewHelper>)ElementToDiagramCommandFactory<Comment, CommentViewHelper>.Factory().Create(Controller);
			d.IncludedElement = c.CreatedComment;
			d.ViewHelper.X = X;
			d.ViewHelper.Y = Y;

			this.ViewHelper = d.ViewHelper;

			Commands.Add(c);
			Commands.Add(d);
		}

		public override void CommandsExecuted()
		{
			base.CommandsExecuted();
			AssociatedElements.Add(d.IncludedElement.Element);
		}
	}

	#region NewModelCommentaryToDiagramCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelCommentToDiagramCommand"/>.
	/// Derived from <see cref="DiagramCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="DiagramController"/> during their initialization.
	/// </summary>
	public class NewModelCommentaryToDiagramCommandFactory : DiagramCommandFactory<NewModelCommentaryToDiagramCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelCommentaryToDiagramCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelCommentToDiagramCommand
		/// <param name="diagramController">Associated diagram controller</param>
		/// </summary>
		public override IStackedCommand Create(DiagramController diagramController)
		{
			return new NewModelCommentToDiagramCommand(diagramController) { AnnotatedElement = diagramController.ModelController.Model };
		}
	}

	#endregion
}