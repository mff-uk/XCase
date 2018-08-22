using XCase.Controller.Commands;
using XCase.Model;

namespace XCase.Controller
{
    /// <summary>
    /// This is a Controller for a Commentary used to receive requests from View and create commands
    /// for changing the model accordingly
    /// </summary>
    public class CommentController : ElementController
	{
		public Comment Comment { get { return ((Comment)Element); } }

		public CommentController(Comment comment, DiagramController diagramController) :
			base(comment, diagramController)
		{
		}

		public void ChangeComment(string newComment)
		{
			ChangeCommentCommand command = (ChangeCommentCommand)ChangeCommentCommandFactory.Factory().Create(DiagramController.ModelController);
			command.ChangedComment = Comment;
			command.NewComment = newComment;
			command.Execute();
		}
	}
}