using System;
using System.Windows;
using System.Windows.Input;
using XCase.Controller.Commands;

namespace XCase.Controller.Dialogs
{
    public partial class ErrDialog : Window
    {
        public ErrDialog()
        {
            InitializeComponent();
        }

        private void bClose_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }

        public void SetText(string description, string message)
        {
            tbCommand.Text = description;
            tbExMsg.Text = message;
        }
    }
}