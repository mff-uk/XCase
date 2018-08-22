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
    ///  ContentContainerGrid displays properties of a content container selected on the canvas
    /// </summary>
    public partial class ContentContainerGrid : XCaseGridBase
    {
        RenameElementConverter renameElement = null;

        /// <summary>
        /// Initialization
        /// </summary>
        public ContentContainerGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent()
        {
            if (elementNameBox.IsFocused)
            {
                BindingExpression be = elementNameBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }

            renameElement = null;
        }

        /// <summary>
        /// Displays selected content container in the Properties window
        /// </summary>
        /// <param name="c"></param>
        public void Display(PSM_ContentContainer c)
        {
            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "renameElement")
                {
                    renameElement = (RenameElementConverter)ie.Value;
                    renameElement.Controller = c.Controller;
                }

            }

            elementNameBox.DataContext = c.Controller.NamedElement;

            appearance.SetElement(c.Controller.DiagramController, c.ViewHelper);
            appearance.DisableEdit();

        }

        #region Event handlers

        private void elementNameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
        }

        private void ContentContainerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                elementNameBox.Width = e.NewSize.Width - 70;
                e.Handled = true;
            }
        }

        private void elementNameBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            appearance.ExtendWidth(e.NewSize.Width);
        }

        #endregion
    }
}
