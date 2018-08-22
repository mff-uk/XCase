using System;

namespace XCase.UMLController.Commands
{
	/// <summary>
	/// Thrown when some field of a command is not properly initialized
	/// </summary>
	public class CommandNotInitializedException: ArgumentException
	{
		public CommandBase Command { get; protected set; }



		public CommandNotInitializedException(CommandBase command, string paramName, string message, Exception innerException)
			: base(message, paramName, innerException)
		{
			Command = command;
		}

		public CommandNotInitializedException(CommandBase command, string paramName, string message)
			: base(message, paramName)
		{
			Command = command;
		}

		public CommandNotInitializedException(CommandBase command, string paramName)
			: base(String.Format(CommandError.CMDERR_MANDATORY_ARGUMENT_NOT_INITIALIZED_2, command.ToString(), paramName), paramName)
		{
			Command = command;
		}
	}
}