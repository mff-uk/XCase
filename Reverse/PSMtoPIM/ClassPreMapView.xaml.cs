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
using System.Windows.Shapes;
using XCase.Model;

namespace XCase.Reverse
{
    /// <summary>
    /// Interaction logic for ClassPreMapView.xaml
    /// </summary>
    public partial class ClassPreMapView : Window
    {
        public Dictionary<PSMClass, PSMtoPIM.ClassPreMaping> Mapping { set { lbxMapping.DataContext = value; } }
        
        public ClassPreMapView()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
