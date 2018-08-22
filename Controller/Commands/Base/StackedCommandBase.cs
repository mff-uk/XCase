using System.Diagnostics;
using XCase.Controller.Dialogs;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Abstract command, all other Commands with undo/redo semantics that 
    /// should be put to the "Undo"/"Redo" stacks
    /// should inherit from this base class. Prescribes 
    /// command infrastructure, handles command stacks.
    /// Deriving from <see cref="StackedCommandBase{ControllerType}"/> is possible, but 
    /// deriving from its subclasses <see cref="ModelCommandBase"/>, <see cref="DiagramCommandBase"/>
    /// and <see cref="MacroCommand{ControllerType}"/> is more convenient.
    /// </summary>
    /// <typeparam name="ControllerType">Type of the controller of the command. 
    /// Usually <see cref="DiagramController"/> or <see cref="ModelController"/></typeparam>
    /// <remarks>
    /// Inheriting classes have to implement <see cref="CommandBase.CommandOperation"/> and 
    /// <see cref="CommandBase.UndoOperation"/>
    /// and <see cref="CommandBase.CanExecute"/> methods. 
    /// <see cref="CommandBase.OnCanExecuteChanged"/> should be called each time some
    /// changes occur that affect whether or not the command should execute.
    /// </remarks>
    /// <seealso cref="DiagramCommandBase"/>
    /// <seealso cref="ModelCommandBase"/>
    /// <seealso cref="MacroCommand{ControllerType}"/>
	public abstract class StackedCommandBase<ControllerType>
		: CommandBase, IStackedCommand where ControllerType : CommandControllerBase
    {
		/// <summary>
		/// Cotroller of the command, handles command execution and contains 
		/// stacks. 
		/// </summary>
		public ControllerType Controller { get; private set; }

		/// <summary>
		/// Cotroller of the command, handles command execution and contains 
		/// command stacks. 
		/// </summary>
    	CommandControllerBase IStackedCommand.Controller { get { return Controller; } }

		/// <summary>
		/// Gets <see cref="Controller"/>'s UndoStack
		/// </summary>
        protected CommandStack UndoStack
        {
            get { return Controller.getUndoStack(); }
        }

		/// <summary>
		/// Gets <see cref="Controller"/>'s RedoStack
		/// </summary>
        protected CommandStack RedoStack
        {
            get { return Controller.getRedoStack(); }
        }
		
		/// <summary>
		/// Creates new instance of StackedCommandBase. 
		/// </summary>
		/// <param name="controller">cotroller of the command, handles command execution and contains command stacks</param>
		protected StackedCommandBase(ControllerType controller)
		{
			Controller = controller;
		}

    	/// <summary>
    	/// Executes the command with the given parameter. This means calling 
    	/// <see cref="CommandBase.CommandOperation"/>,
    	/// pushing the command on the "Undo" stack and clearing the "Redo" stack.
    	/// </summary>
		public override sealed void Execute()
		{
			if (Controller.CreatedMacro != null)
			{
				Controller.CreatedMacro.Commands.Add(this);
				return;
			}

			CommandNumber = getNextCommandNumber();
			// call notifying method
			Controller.OnExecutingCommand(this, false, null);
			// check mandatory arguments
#if DEBUG
			FieldsChecker.CheckMandatoryArguments(this);
#endif
			if (!CommandCantExecuteDialog.CheckCanExecute(this))
			{
				return;
			}
            if (Undoable)
            {
                UndoStack.Push(this);
                // all redo Commands are from now invalid
                RedoStack.Invalidate();
            }
            // call the actual executive method
			CommandOperation();
			Debug.WriteLine(string.Format("Command {0} executed.", this));
#if DEBUG
			FieldsChecker.CheckCommandResults(this);
#endif
			// call successful notification method
			Controller.OnExecutedCommand(this, false, null);
		}

    	/// <summary>
    	/// Executes the command as "Redo", which is the same as <see cref="Execute"/> does, except it does not
    	/// clear the "Redo" stack.
    	/// </summary>
    	public void ExecuteAsRedo()
    	{
    		Debug.Assert(RedoStack != null && RedoStack.Count > 0 && RedoStack.Peek() == this);
    		RedoStack.Pop();
    		// push command on the undo stack
    		UndoStack.Push(this);
    		if (!CommandCantExecuteDialog.CheckCanExecute(this))
    		{
    			return;
    		}
			// call the actual executive method
    		RedoOperation();
    		Debug.WriteLine(string.Format("Redo of command {0} executed", this));
    	}

    	/// <summary>
    	/// Executes "Undo" of the command. This means removing the command from the "Undo" stack and moving it 
    	/// to the "Redo" stack and calling 
    	/// <see cref="CommandBase.UndoOperation"/>.
    	/// </summary>
    	public override sealed void UnExecute()
    	{
    		// assert the command is at the top of the undo stack 
    		Debug.Assert(UndoStack != null && UndoStack.Count > 0 && UndoStack.Peek() == this);
    		// pop command from the undo stack
    		UndoStack.Pop();
    		// push command to the redo stack 
    		// call the actual undo method
    		if (UndoOperation() == OperationResult.OK)
    		{
    			Debug.WriteLine(string.Format("Undo of command {0} executed", this));
				RedoStack.Push(this);
    		}
    		else
    		{
    			Debug.WriteLine(string.Format("ERROR: undo of command {0} failed!", this));
                ErrDialog d = new ErrDialog();
                d.tbCommand.Text = string.Format("Undo of command {0} failed.", this);
                d.tbExMsg.Text = this.ErrorDescription;
                d.ShowDialog();
				UndoStack.Invalidate();
    		}
    	}
    }
}
