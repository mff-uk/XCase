using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
namespace XCase.Reverse
{
    /// <summary>
    /// Interaction logic for PSMtoPIMWindow.xaml
    /// </summary>
    public partial class PSMtoPIMWindow : Window
    {

        public PSMtoPIM PSMtoPIM;
        
        public PSMtoPIMWindow(PSMtoPIM psmToPIM)
        {
            InitializeComponent();
            PSMtoPIM = psmToPIM;
        }

    }
}
