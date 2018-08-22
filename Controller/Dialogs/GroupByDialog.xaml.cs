using System.Windows;

namespace XCase.Controller.Dialogs
{
	/// <summary>
    /// Interaction logic for GroupByDialog.xaml
	/// </summary>
	public partial class GroupByDialog 
	{

        public GroupByDialog()
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
