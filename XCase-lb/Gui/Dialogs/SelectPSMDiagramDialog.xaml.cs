using System.Windows;

namespace XCase.Gui.Dialogs
{
	/// <summary>
    /// Interaction logic for SelectPSMDiagramDialog.xaml
	/// </summary>
	public partial class SelectPSMDiagramDialog 
	{

        public SelectPSMDiagramDialog()
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
