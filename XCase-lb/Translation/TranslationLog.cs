using System.Collections.Generic;

namespace XCase.Translation
{
	/// <summary>
	/// Log where errors and warnings that occur during translation 
	/// are inserted. 
	/// </summary>
	public class TranslationLog : List<LogMessage>
	{
		private int errors = 1;

		private int warnings = 1;

		/// <summary>
		/// Adds error message.
		/// </summary>
		/// <param name="text">The message text.</param>
		public void AddError(string text)
		{
			Add(new LogMessage { MessageText = text, Severity = LogMessage.ESeverity.Error, Number = errors});
			errors++;
		}

		/// <summary>
		/// Adds warning message.
		/// </summary>
		/// <param name="text">The message text.</param>
		public void AddWarning(string text)
		{
			Add(new LogMessage { MessageText = text, Severity = LogMessage.ESeverity.Warning, Number = warnings});
			warnings++;
		}

		/// <summary>
		/// Removes all elements from the log.
		/// </summary>
		public new void Clear()
		{
			base.Clear();
			errors = 1;
			warnings = 1;
		}
	}

}