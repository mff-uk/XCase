using System.Diagnostics;
using System.Text;
using System.Windows.Controls;

namespace XCase.Gui.LogWindow
{
	public class WindowTraceListener: TraceListener 
	{
		public TextBox TextBox { get; set; }

		readonly StringBuilder sb = new StringBuilder();

		public override void Write(string message)
		{
			sb.Append(message);
			TextBox.Text = sb.ToString();
			ScrollViewer s = TextBox.Parent as ScrollViewer;
			if (s != null)
				s.ScrollToEnd();
		}

		public override void WriteLine(string message)
		{
			sb.AppendLine(message);
			TextBox.Text = sb.ToString();
			ScrollViewer s = TextBox.Parent as ScrollViewer;
			if (s != null)
				s.ScrollToEnd();
		}
	}
}