using System;
using System.Collections.Generic;
using System.Windows;
using XCase.Model;

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

        public SelectPSMDiagramDialog(IEnumerable<PSMDiagram> diagrams)
            : this()
        {
            cmbDiagram.ItemsSource = diagrams;
            cmbDiagram.SelectedIndex = 0;
        }

	    public PSMDiagram SelectedDiagram
	    {
	        get { return cmbDiagram.SelectedValue as PSMDiagram; }
	        set { cmbDiagram.SelectedValue = value; }
	    }

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
	}
}
