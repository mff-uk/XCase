using System.Windows;

namespace XCase.Controller.Dialogs
{
	/// <summary>
    /// Interaction logic for SelectRepresentedClassDialog.xaml
	/// </summary>
	public partial class SelectRepresentedClassDialog 
	{

        public SelectRepresentedClassDialog()
		{
			InitializeComponent();
		}

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
	}
}
