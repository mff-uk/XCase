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
    /// Interaction logic for OfferPIMAttributeWindow.xaml
    /// </summary>
    public partial class OfferPIMAttributeWindow : Window
    {
        /// <summary>
        /// Stores the offer for display in the listbox and selects the first attribute
        /// </summary>
        public List<KeyValuePair<Property, double>> Offer { set { lbxOffer.DataContext = value; lbxOffer.SelectedIndex = 0; } }

        /// <summary>
        /// Returns the selected Property (Attribute)
        /// </summary>
        public Property Selected { get { return lbxOffer.SelectedValue as Property; } }

        /// <summary>
        /// True - the user wants to create a new PIM attribute
        /// </summary>
        public bool NewPIM = false;

        public OfferPIMAttributeWindow()
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
