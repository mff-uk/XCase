using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XCase.Model;
using System.Linq;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for IncludeElementsDialog.xaml
	/// </summary>
	public partial class OkCancelDialog : Window
	{
		public OkCancelDialog()
		{
			InitializeComponent();
		}

        public string PrimaryContent
		{
			get
			{
                return label1.Text;
			}
			set
			{
				label1.Text = value;
			}
		}

		public string SecondaryContent
		{
			get
			{
                return label2.Text;
			}
			set
			{
                label2.Text = value;
			}
		}

		public object CancelButtonContent
		{
			get
			{
				return bCancel.Content;
			}
			set
			{
				bCancel.Content = value;
			}
		}

		public object OkButtonContent
		{
			get
			{
				return bOK.Content;
			}
			set
			{
				bOK.Content = value;
			}
		}

	    public Visibility OkButtonVisibility
	    {
            get
            {
                return bOK.Visibility;
            }
            set
            {
                bOK.Visibility = value;
            }
	    }

        public Visibility CancelButtonVisibility
        {
            get
            {
                return bCancel.Visibility;
            }
            set
            {
                bCancel.Visibility = value;
            }
        }

		private void bOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Close();
		}

		private void bCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}
	}
}
