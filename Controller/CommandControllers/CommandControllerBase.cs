using XCase.Controller.Commands;

namespace XCase.Controller
{
    /// <summary>
    /// Controller base
    /// </summary>
    public abstract class CommandControllerBase
    {
        /// <summary>
        /// This is the accessor of the UndoStack, which is actually located in the ModelController.
        /// </summary>
        /// <returns>The Undo Stack</returns>
        public abstract CommandStack getUndoStack();

        /// <summary>
        /// This is the accessor of the RedoStack, which is actually located in the ModelController.
        /// </summary>
        /// <returns>The Redo Stack</returns>
        public abstract CommandStack getRedoStack();
        
        /// <summary>
        /// This method raises the ExecutingCommand event
        /// </summary>
        /// <param name="commandBase">Either DiagramCommandBase or ModelCommandBase</param>
        /// <param name="isPartOfMacro">Tells whether this command is or is not a part of a MacroCommand</param>
        /// <param name="macroCommand">References the MacroCommand of which this command is a part</param>
        public void OnExecutingCommand(CommandBase commandBase, bool isPartOfMacro, CommandBase macroCommand)
    	{
    		if (ExecutingCommand != null)
    		{
    			ExecutingCommand(commandBase, isPartOfMacro, macroCommand);
    		}
    	}

        /// <summary>
        /// This method raises the ExecutedCommand event
        /// </summary>
        /// <param name="commandBase">Either DiagramCommandBase or ModelCommandBase</param>
        /// <param name="isPartOfMacro">Tells whether this command is or is not a part of a MacroCommand</param>
        /// <param name="macroCommand">References the MacroCommand of which this command is a part</param>
        public void OnExecutedCommand(CommandBase commandBase, bool isPartOfMacro, CommandBase macroCommand)
    	{
    		if (ExecutedCommand != null)
    		{
    			ExecutedCommand(commandBase, isPartOfMacro, macroCommand);
    		}
    	}

    	/// <summary>
    	/// All commands executed after this call will be stored in a queue and executed when CommitMacro is called
    	/// </summary>
    	/// <returns>The MacroCommand created</returns>
        public abstract IMacroCommand BeginMacro();

        /// <summary>
        /// The created MacroCommand from the BeginMacro/CommitMacro call pair
        /// </summary>
        protected internal IMacroCommand CreatedMacro { get; protected set; }
        
        /// <summary>
        /// Executes all commands stored after the BeginMacro call
        /// </summary>
        public void CommitMacro()
    	{
			IMacroCommand tmp = CreatedMacro;
			CreatedMacro = null;
			if (tmp.Commands.Count > 0)
				tmp.Execute();    		
    	}

    	/// <summary>
    	/// All stored commands after the BeginMacro call are thrown away
    	/// </summary>
        public void CancelMacro()
		{
			CreatedMacro = null;
		}

    	/// <summary>
    	/// Raised when a command starts executing
    	/// </summary>
        public event CommandEventHandler ExecutingCommand;
    	
		/// <summary>
		/// Raised when a command finished executing
		/// </summary>
        public event CommandEventHandler ExecutedCommand;
    }

	/// <summary>
	/// Event handler of ExecutingCommand/ExecutedCommand events
	/// </summary>
	/// <param name="command">Either DiagramCommandBase or ModelCommandBase</param>
    /// <param name="isPartOfMacro">Tells whether this command is or is not a part of a MacroCommand</param>
    /// <param name="macroCommand">References the MacroCommand of which this command is a part</param>
    public delegate void CommandEventHandler(CommandBase command, bool isPartOfMacro, CommandBase macroCommand);
}
