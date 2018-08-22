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
    /// Interaction logic for OfferPIMClassWindow.xaml
    /// </summary>
    public partial class OfferPIMClassWindow : Window
    {
        public List<Tuple<PIMClass, PSMtoPIM.PIMOffer>> Offer { set { lbxOffer.DataContext = value; lbxOffer.SelectedIndex = 0; } }

        public PIMClass Selected { get { return lbxOffer.SelectedValue as PIMClass; } }

        public bool NewPIM = false;

        public OfferPIMClassWindow()
        {
            InitializeComponent();
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            NewPIM = true;
            DialogResult = true;
            Close();
        }
    }
}
