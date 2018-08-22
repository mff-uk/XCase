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

namespace XCase.Gui.Dialogs
{
    /// <summary>
    /// Interaction logic for ErrMsgBox.xaml
    /// </summary>
    public partial class ErrMsgBox : Window
    {
        public ErrMsgBox()
        {
            InitializeComponent();
        }

        private static ErrMsgBox msgBox;

        public static void Show(string messageText, string messageQuestion)
        {
            msgBox = new ErrMsgBox();           
            msgBox.messageText.Text = messageText;
            msgBox.messageQuestion.Text = messageQuestion;
            msgBox.ShowDialog();
            return;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            msgBox.Close();
        }
    }
}
