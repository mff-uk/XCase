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
using XCase.Controller;
using System.ComponentModel;

namespace XCase.Reverse
{
    /// <summary>
    /// Interaction logic for ReverseLog.xaml
    /// </summary>
    public partial class ReverseLog : Window
    {
        public TextBlock TextBlock { get { return textBlock; } }

        public DiagramController controller;
        
        public ReverseLog()
        {
            InitializeComponent();
        }

        public string Filename { get; set; }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ReverseEngineering R = new ReverseEngineering(controller);
            R.DeleteUnnecessarySRsMadeByExtensions = chkDeleteRootSRsByExt.IsChecked == true;
            R.ResolveSRs = chkResolveSRs.IsChecked == true;
            R.UseCommands = chkCommands.IsChecked == true;
            R.Layout = chkLayout.IsChecked == true;
            bStart.Content = "WAIT UNTIL THE ALGORITHM ENDS";
            bStart.IsEnabled = false;

            CancelEventHandler H = new CancelEventHandler(ReverseLog_Closing);
            Closing += H;
            R.XSDtoPSM(TextBlock, lblCount, pbProgress, Filename);
            bStart.Content = "FINISHED (click to run again)";
            bStart.IsEnabled = true;
            Closing -= H;
        }

        void ReverseLog_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
