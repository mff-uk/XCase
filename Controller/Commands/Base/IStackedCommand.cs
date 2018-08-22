using System;
using System.Collections.Generic;
using System.Windows.Input;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Interface for stacked commands, commands that are placed on the stack when executed
	/// and can be unexecuted by executing <see cref="UndoCommand"/> and reexecuted by 
	/// executing <see cref="RedoCommand"/>
	/// </summary>
	/// <seealso cref="StackedCommandBase{ControllerType}"/>
	public interface IStackedCommand
	{
		/// <summary>
		/// Returns description of the command
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Cotroller of the command, handles command execution and contains 
		/// stacks. 
		/// </summary>
		CommandControllerBase Controller { get; }

		/// <summary>
		/// Number of the command. 
		/// </summary>
		/// <seealso cref="CommandBase.getNextCommandNumber"/>
		int? CommandNumber { get; }

		/// <summary>
		/// This event should be raised each some condition that determines whether 
		/// the command can execute or not changes
		/// </summary>
		event EventHandler CanExecuteChanged;

		/// <summary>
		/// This method should be called each time some
		/// changes occur that affect whether or not the command should execute.
		/// </summary>
		/// <param name="sender">subscribers of the <see cref="CanExecuteChanged"/> 
		/// event will get this value in the sender argument</param>
		/// <param name="args">subscribers of the <see cref="CanExecuteChanged"/> 
		/// event will get this value in the args argument</param>
		void OnCanExecuteChanged(object sender, EventArgs args);

		/// <summary>
		/// Elements associated with the command. 
		/// </summary>
		IList<Element> AssociatedElements { get; }

		/// <summary>
		/// Executes the command
		/// </summary>
		void Execute();

		/// <summary>
		/// Executes the command as "Redo", which is the same as <see cref="CommandBase.Execute()"/> does, except it does not
		/// clear the "Redo" stack.
		/// </summary>
		void ExecuteAsRedo();

		/// <summary>
		/// Executes "Undo" of the command. This means removing the command from the "Undo" stack and moving it 
		/// to the "Redo" stack and calling 
		/// <see cref="CommandBase.UndoOperation"/>.
		/// </summary>
		void UnExecute();
	}
}