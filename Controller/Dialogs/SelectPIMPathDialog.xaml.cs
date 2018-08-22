
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Effects;
using System.Diagnostics;
using XCase.Model;

namespace XCase.Controller.Dialogs
{
	/// <summary>
	/// Interaction logic for SelectPIMPathDialog.xaml
	/// </summary>
	public partial class SelectPIMPathDialog 
	{
        public delegate TreeClasses AddChildrenDelegate(TreeClasses TC, List<PIMClass> Paths, PIMClass Original, List<Generalization> UsedGeneralizations, int depth);

        public AddChildrenDelegate AddChildren;

        public SelectPIMPathDialog()
		{
			InitializeComponent();
		}

        private void bOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (TreeClasses T in ((sender as MenuItem).DataContext as TreeClasses).Children)
            {
                T.selected = true;
            }
        }

        private void DeselectAllClick(object sender, RoutedEventArgs e)
        {
            foreach (TreeClasses T in ((sender as MenuItem).DataContext as TreeClasses).Children)
            {
                T.selected = false;
            }
            
            //DEMO FOR DYNAMIC TREEVIEWITEM ADDITION
            //TreeClasses Ta = ((sender as MenuItem).DataContext as TreeClasses);
            //Ta.Children.Add(new TreeClasses() { PIMClass = Ta.PIMClass, Children = null, Association = Ta.Association, Lower = Ta.Lower, Upper = Ta.Upper, selected = true });
        }

        private void treeView_Expanded(object sender, RoutedEventArgs e)
        {
            if ((e.OriginalSource as TreeViewItem).Header is TreeClasses)
            {
                TreeClasses TC = ((e.OriginalSource as TreeViewItem).Header as TreeClasses);

                Debug.WriteLine("Expanded " + TC.PIMClass);

                if (AddChildren != null)
                {
                    foreach (TreeClasses T in TC.Children)
                    {
                        if (!T.ChildrenAdded) AddChildren(T, T.RecursionStatus.Paths, T.RecursionStatus.Original, T.RecursionStatus.UsedGeneralizations, 1);
                    }
                }
            }
        }

        private void treeView_Selected(object sender, RoutedEventArgs e)
        {
            (e.OriginalSource as TreeViewItem).IsSelected = false;
        }
	}
}
