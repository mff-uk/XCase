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
using System.Windows.Navigation;
using System.Windows.Shapes;
using XCase.View.Controls;
using XCase.Controller.Commands;
using XCase.Controller;
using XCase.Model;
using System.Collections;
using System.Text.RegularExpressions;

namespace XCase.Gui
{
    /// <summary>
    /// Appearance grid displays appearance properties of a PIM or PSM visual element.
    /// These appearance properties are:
    /// - X coordinate
    /// - Y coordinate
    /// - Width
    /// - Height
    /// Appearance grid is meant to be used within the Properties window.
    /// </summary>
    public partial class AppearanceGrid : XCaseGridBase
    {
        DiagramController dc = null;
        PositionableElementViewHelper vh = null;

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent()
        {
            if (widthBox.IsFocused)
            {
                ForcePositiveNumValue(widthBox, null);
                BindingExpression be = widthBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
            else
            if (heightBox.IsFocused)
            {
                ForcePositiveNumValue(heightBox, null);
                BindingExpression be = heightBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
            else
            if (xBox.IsFocused)
            {
                ForceNumValue(xBox, null);
                BindingExpression be = xBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
            else
            if (yBox.IsFocused)
            {
                ForceNumValue(yBox, null);
                BindingExpression be = yBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }

        /// <summary>
        /// Initialization
        /// </summary>
        public AppearanceGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Set element of which appearance properties are displayed
        /// </summary>
        /// <param name="d"></param>
        /// <param name="h"></param>
        public void SetElement(DiagramController d, PositionableElementViewHelper h)
        {
            vh = h;
            dc = d;

            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "widthConverter")
                {
                    WidthConverter widthConverter = (WidthConverter)ie.Value;
                    widthConverter.diagramController = dc;
                    widthConverter.viewHelper = vh;
                }
                else
                if (ie.Key.ToString() == "heightConverter")
                {
                    HeightConverter heightConverter = (HeightConverter)ie.Value;
                    heightConverter.diagramController = dc;
                    heightConverter.viewHelper = vh;
                }
                else
                if (ie.Key.ToString() == "xConverter")
                {
                    XConverter xConverter = (XConverter)ie.Value;
                    xConverter.diagramController = dc;
                    xConverter.viewHelper = vh;
                }
                else
                if (ie.Key.ToString() == "yConverter")
                {
                    YConverter yConverter = (YConverter)ie.Value;
                    yConverter.diagramController = dc;
                    yConverter.viewHelper = vh;
                }
            }
          
            widthBox.DataContext = h;
            heightBox.DataContext = h;
            xBox.DataContext = h;
            yBox.DataContext = h;
        }

        /// <summary>
        /// Checks whether the new value is a correct positive integer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForcePositiveNumValue(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).IsReadOnly)
                return;

            object oldContext = ((TextBox)sender).DataContext;
            Regex numbers = new Regex("^[0-9]+$");

            Match m = numbers.Match(((TextBox)sender).Text);
            if (!m.Success)
            {
                ((TextBox)sender).DataContext = null;
                ((TextBox)sender).DataContext = oldContext;
            }

       }

        /// <summary>
        /// Checks whether the new value is a correct integer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForceNumValue(object sender, RoutedEventArgs e)
        {
            if (((TextBox)sender).IsReadOnly)
                return;

            object oldContext = ((TextBox)sender).DataContext;
          
            int i;
            if (!int.TryParse(((TextBox)sender).Text, out i))
            {
                ((TextBox)sender).DataContext = null;
                ((TextBox)sender).DataContext = oldContext;
            }
        }

        /// <summary>
        /// Sets all textboxes of the Appearance grid as read-only
        /// </summary>
        public void DisableEdit()
        {
            widthBox.IsReadOnly = true;
            heightBox.IsReadOnly = true;
            xBox.IsReadOnly = true;
            yBox.IsReadOnly = true;  
        }

        /// <summary>
        /// Sets width of the grid according to new value
        /// </summary>
        /// <param name="newWidth">New width</param>
        public void ExtendWidth(double newWidth)
        {
            secondColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength(newWidth,GridUnitType.Pixel));
        }

        #region Event handlers

        /// <summary>
        /// Occurs when a key is pressed up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        new protected void KeyPressed_Up(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left || e.Key == Key.Back)
                e.Handled = true;

            if (e.Key == Key.Down)
                ((Control)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
            else
                KeyPressed_Up_Last(sender, e);
        }

        /// <summary>
        /// Occurs when a key is pressed down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        new protected void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;

            else
                // Confirm value
                if (e.Key == Key.Enter)
                {
                    if (((TextBox)sender).Name.Equals("xBox") || ((TextBox)sender).Name.Equals("yBox"))
                        ForceNumValue(sender, null);
                    else
                        ForcePositiveNumValue(sender, null);

                    BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();

                    //((Control)sender).Focus();
                }
        }

        /// <summary>
        /// Occurs when a key is pressed down in the last textbox of the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyPressed_Up_Last(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;
            else
                if (e.Key == Key.Up && !((TextBox)sender).Name.Equals("widthBox"))
                    ((Control)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
        }

        #endregion
    }
}
