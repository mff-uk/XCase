using System;
using System.Diagnostics;
using XCase.Controller.Commands.Helpers;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Adds new comment to a directory
	/// </summary>
	public class NewModelCommentCommand : ModelCommandBase
	{
        /// <summary>
        /// Index from which is removed in Undo
        /// </summary>
        private int Index;
        
        /// <summary>
		/// New created comment
		/// </summary>
		public ElementHolder<Comment> CreatedComment { get; set; }

		/// <summary>
		/// Comment text
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Element that the commmentary annotates. Field is optional and commentary can be placed
		/// freely on the diagram without being connected to a specific element.
		/// </summary>
		public Element AnnotatedElement { get; set; }

	
		/// <summary>
		/// Creates new instance of <see cref="NewModelCommentCommand" />. 
		/// </summary>
		/// <param name="modelController">controller of the command</param>
		public NewModelCommentCommand(ModelController modelController)
			: base(modelController)
		{
			Description = CommandDescription.ADD_MODEL_COMMENT;
		}

		public override bool CanExecute()
		{
			if (AnnotatedElement == null)
			{
				ErrorDescription = CommandError.CMDERR_COMMENT_ANNOTATED_ELEMENT_MISSING;
				return false;
			}
			return true;
		}

		internal override void CommandOperation()
		{
			if (CreatedComment == null)
				CreatedComment = new ElementHolder<Comment>();
			CreatedComment.Element = AnnotatedElement.AddComment(NameSuggestor<Comment>.SuggestUniqueName(AnnotatedElement.Comments, "Comment", comment => comment.Body)); ;
			
			CreatedComment.Element.Body = Text;
			
			Debug.Assert(CreatedComment.HasValue);
			AssociatedElements.Add(CreatedComment.Element);
		}

		internal override OperationResult UndoOperation()
		{
			if (!AnnotatedElement.Comments.Contains(CreatedComment.Element))
			{
                ErrorDescription = String.Format(CommandError.CMDERR_NOT_FOUND, CreatedComment.Element);
                return OperationResult.Failed;
			}
			if (Controller.IsElementUsedInDiagrams(CreatedComment.Element))
			{
                ErrorDescription = CommandError.CMDERR_REMOVED_ELEMENT_BEING_USED;
                return OperationResult.Failed;
			}
            Index = AnnotatedElement.Comments.IndexOf(CreatedComment.Element);
            AnnotatedElement.Comments.Remove(CreatedComment.Element);
			return OperationResult.OK;
		}

        internal override void RedoOperation()
        {
            AnnotatedElement.Comments.Insert(Index, CreatedComment.Element);
        }
	}

	#region NewModelCommentCommandFactory

	/// <summary>
	/// Factory that creates instances of <see cref="NewModelCommentCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class NewModelCommentCommandFactory : ModelCommandFactory<NewModelCommentCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
		private NewModelCommentCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of NewModelCommentCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
			return new NewModelCommentCommand(modelController);
		}
	}

	#endregion

}
