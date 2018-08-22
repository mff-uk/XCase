using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Diagnostics;

namespace XCase.Controller.Commands
{
    /// <summary>
    /// Performs "Redo" of the last command on the "Redo" stack. 
    /// </summary>
	/// <remarks>This command is never put on any of the "Undo" or "Redo" stacks. </remarks>
    /// <seealso cref="UndoCommand"/>
    public class RedoCommand : ICommand, INotifyPropertyChanged
    {
		private readonly CommandStack modelRedoStack;

        private readonly ModelController Controller;
        
        public RedoCommand(CommandStack modelRedoStack, ModelController controller)
		{
			this.modelRedoStack = modelRedoStack;
            Controller = controller;

			modelRedoStack.ItemsChanged += redoStack_ItemsChanged;
        }

        void redoStack_ItemsChanged(object sender, EventArgs e)
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
					return GetRedoneCommand().Description;
				}
				else return null;
			}
		}


		public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
			return modelRedoStack.Count > 0;
        }

        public void Execute(object parameter)
        {
            IStackedCommand redoneCommand = GetRedoneCommand();

        	redoneCommand.ExecuteAsRedo();
            if (redoneCommand is DiagramCommandBase)
            {
                ActivateDiagramCommand c = ActivateDiagramCommandFactory.Factory().Create(Controller) as ActivateDiagramCommand;
                c.Set((redoneCommand as DiagramCommandBase).Controller.Diagram);
                Debug.WriteLine("Changing active diagram due to Redo");
                c.Execute();
            }
            if (redoneCommand is MacroCommand<DiagramController>)
            {
                ActivateDiagramCommand c = ActivateDiagramCommandFactory.Factory().Create(Controller) as ActivateDiagramCommand;
                c.Set((redoneCommand as MacroCommand<DiagramController>).Controller.Diagram);
                Debug.WriteLine("Changing active diagram due to Redo");
                c.Execute();
            }
            if (RedoExecuted != null)
				RedoExecuted(redoneCommand);
		}

    	private IStackedCommand GetRedoneCommand()
    	{
    		IStackedCommand topStackedCommandModel = modelRedoStack.Empty ? null : modelRedoStack.Peek();
    		IStackedCommand redoneCommand;

    		if (topStackedCommandModel != null)
    		{
   				redoneCommand = topStackedCommandModel;
    		}
    		else
    		{
    			throw new InvalidOperationException(CommandError.CMDERR_STACK_INCONSISTENT);
    		}
    		return redoneCommand;
    	}

    	public event Action<IStackedCommand> RedoExecuted;

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
    }
}
