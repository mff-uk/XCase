using System.Collections.Generic;
using System.Text;
using System.Linq;
using XCase.Controller.Dialogs;
using XCase.Model;

namespace XCase.Controller.Commands
{
	/// <summary>
    /// Command that consists of other Commands
    /// </summary>
    public class MacroCommand<ControllerType>: StackedCommandBase<ControllerType>, IMacroCommand
    	where ControllerType : CommandControllerBase
    {
    	public MacroCommand(ControllerType controller)
    		: base(controller)
    	{
			Commands = new List<CommandBase>();
    	}

		public List<CommandBase> Commands { get; private set; }

		public override IList<Element> AssociatedElements
		{
			get
			{
				if (associatedElements.Count == 0
					&& Commands.Any(commandBase => commandBase.AssociatedElements.Count > 0))
				{
					List<Element> elements = new List<Element>();
					foreach (CommandBase command in Commands)
					{
						elements.AddRange(command.AssociatedElements);
					}
					return elements;
				}
				return base.AssociatedElements;
			}
		}

		public bool CheckFirstOnlyInCanExecute { get; set; }

		public bool CanExecuteFirst()
		{
            if (Commands.Count > 0)
            {
                if (Commands[0].CanExecute())
                    return true;
                else
                {
                    ErrorDescription = Commands[0].ErrorDescription;
                    return false;
                }
            }
            else return false;
		}

		/// <summary>
        /// Returns true if all partial Commands can execute.
        /// </summary>
        /// <returns>Returns true if all partial Commands can execute.</returns>
        public override bool CanExecute()
		{
			int count = CheckFirstOnlyInCanExecute ? 1 : Commands.Count;
			for (int i = 0; i < count; i++)
			{
				#if DEBUG
				FieldsChecker.CheckMandatoryArguments(Commands[i]);
				#endif
				if (!(Commands[i].CanExecute()))
				{
					ErrorDescription = Commands[i].ErrorDescription;
					return false;
				}
			}
			return true;
		}

		/// <summary>
        /// Performs CommandOperation of all of the partial Commands.
        /// </summary>
        internal override void CommandOperation()
        {
			for (int i = 0; i < Commands.Count; i++)
			{
				Controller.OnExecutingCommand(Commands[i], true, this);
				#if DEBUG 
				FieldsChecker.CheckMandatoryArguments(Commands[i]);
				#endif
				if (!CommandCantExecuteDialog.CheckCanExecute(Commands[i]))
					return;
				Commands[i].CommandOperation();
				#if DEBUG
				FieldsChecker.CheckCommandResults(this);
				#endif
				Controller.OnExecutedCommand(Commands[i], true, this);
			}
			CommandsExecuted();
        }

        /// <summary>
        /// Performs RedoOperation of all of the partial Commands.
        /// </summary>
        internal override void RedoOperation()
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                Controller.OnExecutingCommand(Commands[i], true, this);
                Commands[i].RedoOperation();
                Controller.OnExecutedCommand(Commands[i], true, this);
            }
            CommandsExecuted();
        }

		/// <summary>
		/// Called after all commands from the macro are executed.
		/// Can be used as a finalizer of the command in derived classes, 
		/// but <strong>must not perform any operation in the model</strong>.
		/// </summary>
		public virtual void CommandsExecuted()
		{
			
		}

		/// <summary>
        /// Performs UndoOperation of all of the partial Commands (in reverse order).
        /// </summary>
        internal override OperationResult UndoOperation()
        {
            for (int i = Commands.Count - 1; i >= 0; i--)
            {
                if (Commands[i].UndoOperation() == OperationResult.Failed)
                {
                    ErrorDescription = "Command " + Commands[i].Description + " failed to Undo: " + Commands[i].ErrorDescription;
                    for (int j = i; j < Commands.Count; j++)
                    {
                        //REDO what was undone before the failure
                        if (Commands[j].CanExecute()) Commands[j].CommandOperation();
                    }
                    return OperationResult.Failed;
                }
            }
            return OperationResult.OK;
        }

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder("MacroCommand { ");
			foreach (CommandBase command in Commands)
			{
				sb.Append(command);
				sb.Append(", ");
			}
			sb.Remove(sb.Length - 2, 2);
			sb.Append(" }");
			return sb.ToString();
		}
    }

	/// <summary>
	/// CommandFactory for creating MacroCommand
	/// </summary>
	public class MacroCommandFactory<ControllerType> : CommandFactoryBase<MacroCommandFactory<ControllerType>>
		where ControllerType : CommandControllerBase
	{
		protected MacroCommandFactory()
		{
		}

		/// <summary>
		/// Creates new instance of MacroCommand
		/// <param name="controller">Associated controller</param>
		/// </summary>
		public MacroCommand<ControllerType> Create(ControllerType controller)
		{
			return new MacroCommand<ControllerType>(controller);
		}
	}
}
