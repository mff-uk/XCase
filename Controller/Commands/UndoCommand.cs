using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Performs "Undo" of the last command on the "Undo" stack. 
    /// </summary>
	/// <remarks>This command is never put on any of the "Undo" or "Redo" stacks</remarks>
    /// <seealso cref="RedoCommand"/>
    public class UndoCommand: ICommand, INotifyPropertyChanged
    {
    	private readonly CommandStack modelUndoStack;

        private readonly ModelController Controller;

        public UndoCommand(CommandStack modelUndoStack, ModelController controller)
        {
            this.modelUndoStack = modelUndoStack;
            Controller = controller;

        	modelUndoStack.ItemsChanged += undoStack_ItemsChanged;
        }

        void undoStack_ItemsChanged(object sender, EventArgs e)
        {
			if (CanExecuteChanged != null)
				CanExecuteChanged(sender, e);
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs("UnderlyingCommandDescription"));
        }

    	public string UnderlyingCommandDescription
    	{
    		get
    		{
    			if (CanExecute(null))
    			{
    				return GetUndoneCommand().Description;
    			}
				else return null;
    		}
    	}

    	public event EventHandler CanExecuteChanged;

    	public bool CanExecute(object parameter)
        {
            return modelUndoStack.Count > 0;
        }

        public void Execute(object parameter)
        {
            IStackedCommand undoneCommand = GetUndoneCommand();

        	undoneCommand.UnExecute();
            if (undoneCommand is DiagramCommandBase)
            {
                ActivateDiagramCommand c = ActivateDiagramCommandFactory.Factory().Create(Controller) as ActivateDiagramCommand;
                c.Set((undoneCommand as DiagramCommandBase).Controller.Diagram);
                Debug.WriteLine("Changing active diagram due to Undo");
                c.Execute();
            }
            if (undoneCommand is MacroCommand<DiagramController>)
            {
                ActivateDiagramCommand c = ActivateDiagramCommandFactory.Factory().Create(Controller) as ActivateDiagramCommand;
                c.Set((undoneCommand as MacroCommand<DiagramController>).Controller.Diagram);
                Debug.WriteLine("Changing active diagram due to Undo");
                c.Execute();
            }
            if (UndoExecuted != null)
				UndoExecuted(undoneCommand);
        }

    	private IStackedCommand GetUndoneCommand()
    	{
    		IStackedCommand topStackedCommandModel = modelUndoStack.Empty ? null : modelUndoStack.Peek();
    		IStackedCommand undoneCommand;

    		if (topStackedCommandModel != null)
    		{
  				undoneCommand = topStackedCommandModel;
    		}
    		else
    		{
    			throw new InvalidOperationException(CommandError.CMDERR_STACK_INCONSISTENT);
    		}
    		return undoneCommand;
    	}

    	public event Action<IStackedCommand> UndoExecuted; 

    	#region Implementation of INotifyPropertyChanged

    	public event PropertyChangedEventHandler PropertyChanged;

    	#endregion
    }
}
