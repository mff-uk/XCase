using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XCase.Gui.MainMenuCommands
{
	/// <summary>
	/// Interaction logic for ToolTipControl.xaml
	/// </summary>
	public partial class ToolTipControl : UserControl
	{
		public string Message
		{
			get
			{
				return tbMessage.Content;
			}
			set
			{
				tbMessage.Content = value;
			}
		}

		public ImageSource ImageLeft
		{
			get
			{
				return ImageLeft.Source;
			}
			set
			{
				ImageLeft.Source = value;
			}
		}

		public ImageSource ImageRight
		{
			get
			{
				return ImageRight.Source;
			}
			set
			{
				ImageRight.Source = value;
			}
		}

		public ToolTipControl()
		{
			InitializeComponent();
		}
	}
}
