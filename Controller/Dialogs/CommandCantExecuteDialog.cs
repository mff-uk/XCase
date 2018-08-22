using System;
using System.Windows;
using System.Windows.Input;
using XCase.Controller.Commands;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Window displaying error in a command. 
	/// Use static <see cref="CheckCanExecute"/> method to call <see cref="CommandBase.CanExecute"/>
	/// and display the info box when the return value is false.
	/// </summary>
	public partial class CommandCantExecuteDialog
	{
		/// <summary>
		/// Window displaying error in a command. 
		/// Use static <see cref="CheckCanExecute"/> method to call <see cref="ICommand.CanExecute"/>
		/// and display the info box when the return value is false.
		/// </summary>
		public CommandCantExecuteDialog()
		{
			InitializeComponent();
		}

		private void bClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Method calls <see cref="ICommand.CanExecute"/> on <paramref name="command"/>
		/// and displays the info box when the return value is false.
		/// </summary>
		/// <param name="command">command</param>
		/// <returns>value returned by <paramref name="command"/>.<see cref="ICommand.CanExecute"/></returns>
		public static bool CheckCanExecute(CommandBase command)
		{
			if (!command.CanExecute())
			{
				CommandCantExecuteDialog dialog = new CommandCantExecuteDialog();
				if (!String.IsNullOrEmpty(command.Description))
				{
					dialog.tbCommand.Content = command.Description;
				}
				else
					dialog.tbCommand.Content = command.ToString();

				dialog.tbExMsg.Content = String.Empty;
				dialog.tbExMsg.Content = command.ErrorDescription;

				dialog.ShowDialog();
				return false;
			}
			return true;
		}
	}
}