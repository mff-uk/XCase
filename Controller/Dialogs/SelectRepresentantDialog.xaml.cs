using System.Windows;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for SelectRepresentantDialog.xaml
	/// </summary>
	public partial class SelectRepresentantDialog 
	{

        public SelectRepresentantDialog()
		{
			InitializeComponent();
            List.SelectionChanged += delegate { List.SelectedIndex = -1; };
		}

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
	}
}
