using System;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Changes text of a comment
	/// </summary>
    public class ChangeCommentCommand : ModelCommandBase 
    {
        public ChangeCommentCommand(ModelController controller)
    		: base(controller)
    	{
        	Description = CommandDescription.CHANGE_COMMENT;
    	}

		/// <summary>
		/// Changed comment
		/// </summary>
    	public Model.Comment ChangedComment { get; set; }

		/// <summary>
		/// New comment text
		/// </summary>
		public string NewComment { get; set; }

    	private string oldComment; 

        public override bool CanExecute()
        {
        	return ((NewComment != null) && (ChangedComment != null));
        }

        internal override void CommandOperation()
        {
        	oldComment = ChangedComment.Body;
        	ChangedComment.Body = NewComment;
        }

        internal override OperationResult UndoOperation()
        {
            ChangedComment.Body = oldComment;
        	return OperationResult.OK;
        }
    }

    #region ChangeCommentCommandFactory

    /// <summary>
    /// Factory that creates instances of <see cref="ChangeCommentCommand"/>.
	/// Derived from <see cref="ModelCommandFactory{ConcreateCommandFactory}"/> 
	/// therefore all its product Commands recieve reference
	/// to <see cref="ModelController"/> during their initialization.
	/// </summary>
	public class ChangeCommentCommandFactory : ModelCommandFactory<ChangeCommentCommandFactory>
	{
		/// <summary>
		/// Direct constructor is hidden to avoid user making standalone instances. 
		/// Static <see cref="CommandFactoryBase{ConcreteCommandFactory}.Factory"/> accessor 
		/// should be used to get the singleton instance of the factory.
		/// </summary>
        private ChangeCommentCommandFactory()
		{
		}

		/// <summary>
        /// Creates new instance of ChangeCommentCommand
		/// <param name="modelController">Associated model controller</param>
		/// </summary>
		public override IStackedCommand Create(ModelController modelController)
		{
            return new ChangeCommentCommand(modelController);
		}
	}

	#endregion
}