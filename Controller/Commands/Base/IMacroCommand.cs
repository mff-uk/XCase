using System.Collections.Generic;

namespace XCase.Controller.Commands
{
	/// <summary>
	/// Extension of <see cref="IStackedCommand"/> interface used for <see cref="MacroCommand{ControllerType}"/>.
	/// Allows usage of an existing <see cref="MacroCommand{ControllerType}"/> instance without knowing the 
	/// ControllerType of the <see cref="MacroCommand{ControllerType}"/>.
	/// </summary>
	public interface IMacroCommand: IStackedCommand
	{
		List<CommandBase> Commands { get; }
		bool CheckFirstOnlyInCanExecute { get; set; }

		/// <summary>
		/// Called after all commands from the macro are executed.
		/// Can be used as a finalizer of the command, but <strong>must not perform any operation in the model</strong>.
		/// </summary>
		void CommandsExecuted();
	}
}