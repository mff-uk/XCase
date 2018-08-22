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
using System.Collections;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;
using System.Text.RegularExpressions;

namespace XCase.Gui
{
    /// <summary>
    /// Interface for xaml grids displayable in the Properties window
    /// </summary>
    public class XCaseGridBase : UserControl
    {
        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public virtual void UpdateContent() { }

        #region Common event handlers

        /// <summary>
        /// Occurs when a key is preesed down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Back)
                e.Handled = true;

            if (e.Key == Key.Enter)
            {
                BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Occurs when a key is preesed up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void KeyPressed_Up(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Back)
                e.Handled = true;
        }

        /// <summary>
        /// Occurs when selection in listview is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView)
              ((ListView)sender).SelectedItems.Clear();
        }

        #endregion       
        
    }
}