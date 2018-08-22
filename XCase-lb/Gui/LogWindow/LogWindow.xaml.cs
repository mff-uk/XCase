using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using XCase.Controller.Commands;

namespace XCase.Gui.LogWindow
{
	/// <summary>
	/// Interaction logic for LogWindow.xaml
	/// </summary>
	public partial class LogWindow
	{
		public LogWindow()
		{
			InitializeComponent();
		}

		public enum ELogDetail
		{
			None,
			Brief,
			FullOneCommand,
			Full
		}

		private ELogDetail logDetail = ELogDetail.Brief;
		public ELogDetail LogDetail
		{
			get
			{
				return logDetail;
			}
			set
			{
				logDetail = value;
			}
		}

		private static readonly Dictionary<Type, PropertyInfo[]> commandLogPropertiesCache = new Dictionary<Type, PropertyInfo[]>();
		private const string Separator = "************************************************";

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			LogDetail = (ELogDetail)Enum.Parse(typeof(ELogDetail), ((ComboBoxItem)((ComboBox)sender).SelectedItem).Content.ToString());
		}

		internal void Controller_CommandExecuted(CommandBase command, bool isPartOfMacro, CommandBase macroCommand)
		{
			if (LogDetail == ELogDetail.None)
				return;

			StringBuilder sb = new StringBuilder(tbCommandLog.Text);

			if (command.CommandNumber != null)
			{
				sb.AppendFormat("Command #{1} Executed ({0}).\r\n\r\n", command, command.CommandNumber);
			}
			else
			{
				sb.AppendFormat("Command Executed ({0}).\r\n", command);
			}
			if (command is IMacroCommand)
			{
				sb.AppendLine(Separator);
				sb.AppendLine(Separator);
			}
			else if (!isPartOfMacro)
			{
				sb.AppendLine(Separator);
			}
			sb.Append("\r\n");
			tbCommandLog.Text = sb.ToString();
			svCommandLog.ScrollToBottom();
		}

		internal void Controller_CommandExecuting(CommandBase command, bool isPartOfMacro, CommandBase macroCommand)
		{
			if (LogDetail == ELogDetail.None)
				return;
			//StringBuilder sb = new StringBuilder(tbCommandLog.Text);
			StringBuilder sb;

			if (LogDetail == ELogDetail.FullOneCommand || LogDetail == ELogDetail.Full)
			{
				if (isPartOfMacro || LogDetail == ELogDetail.Full)
					sb = new StringBuilder(tbCommandLog.Text);
				else
					sb = new StringBuilder();

				if (command is IMacroCommand)
				{

					sb.AppendLine(Separator);
					sb.AppendLine(Separator);
				}
				else
				{
					sb.AppendLine(Separator);
				}
			}
			else
			{
				sb = new StringBuilder(tbCommandLog.Text);
			}

			sb.AppendFormat("Executing command {0}: ", command);
			if (!String.IsNullOrEmpty(command.Description))
			{
				sb.AppendFormat("\r\n  >> {0} <<:", command.Description);
			}
			if (command is IMacroCommand)
			{
				sb.AppendFormat("\r\n\t Macro command with {0} total Commands", (command as IMacroCommand).Commands.Count);
			}
			if (isPartOfMacro)
			{
				sb.AppendFormat("\r\n\t This command is {0}/{1} in the macro", ((IMacroCommand)macroCommand).Commands.IndexOf(command) + 1, ((IMacroCommand)macroCommand).Commands.Count);
			}

			if (LogDetail != ELogDetail.Brief)
			{
				PropertyInfo[] properties;
				if (commandLogPropertiesCache.ContainsKey(command.GetType()))
				{
					properties = commandLogPropertiesCache[command.GetType()];
				}
				else
				{
					properties = command.GetType().GetProperties();
					commandLogPropertiesCache[command.GetType()] = properties;
				}

				foreach (PropertyInfo info in properties)
				{
					if (info.Name != "CommandNumber" && info.Name != "executionParameter" && info.Name != "Description"
						&& info.Name != "CopyOnStack" && info.Name != "ErrorDescription")
					{
						sb.AppendFormat("\r\n\t {0} = {1}", info.Name, info.GetValue(command, null));
					}
				}
			}

			sb.AppendLine("...");
			tbCommandLog.Text = sb.ToString();
			svCommandLog.ScrollToBottom();
		}

		
	}
}