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
using XCase.Controller.Dialogs;

namespace XCase.Gui
{
    /// <summary>
    /// AttributeContainerGrid displays properties of an Attribute container selected on the canvas
    /// </summary>
    public partial class AttributeContainerGrid : XCaseGridBase
    {
        private List<TextBox> attBoxes = new List<TextBox>();

        protected RenameAliasConverter renameAttribute = null;
        private NamedElementController controller = null;

        /// <summary>
        /// Initialization
        /// </summary>
        public AttributeContainerGrid()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent()
        {
            foreach (TextBox t in attBoxes)
            {
                if (t.IsFocused && renameAttribute != null)
                {
                    CheckUniqueAliasName(t, null);

                    renameAttribute.selectedAttribute = (PSMAttribute)(t.DataContext);
                    BindingExpression be = t.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
            }
            renameAttribute = null;
        }

        /// <summary>
        /// Displays Attribute container in the Properties window
        /// </summary>
        /// <param name="c"></param>
        public void Display(PSM_AttributeContainer c)
        {
            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "renameAttribute")
                {
                    renameAttribute = (RenameAliasConverter)ie.Value;
                    renameAttribute.Controller = c.Controller;
                }
            }

            controller = c.Controller;
            attBox.DataContext = c.AttributeContainer.PSMAttributes;

            appearance.SetElement(c.Controller.DiagramController, c.ViewHelper);
            appearance.DisableEdit();

        }

        #region Event handlers

        private void attBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (renameAttribute != null)
                renameAttribute.selectedAttribute = (PSMAttribute)(((TextBox)sender).DataContext);
            
            if (!attBoxes.Contains((TextBox)sender))
            {
                attBoxes.Add((TextBox)sender);
                ((TextBox)sender).Width = grid.Width -35;
            }
        }

        protected void attBox_MouseDoubleClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button && controller is PSM_AttributeContainerController)
                    ((PSM_AttributeContainerController)controller).ShowAttributeDialog((PSMAttribute) ((Control)sender).DataContext);
        }

        private void AttributeContainerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                appearance.ExtendWidth(e.NewSize.Width - 70);
                attBox.Width = e.NewSize.Width - 10;
                foreach (TextBox t in attBoxes)
                    t.Width = e.NewSize.Width - 45;
                e.Handled = true;
            }
        }

        new private void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;

            // Confirmation of inserted value
            if (e.Key == Key.Enter && ((Control)sender).Focusable)
            {
                if (((TextBox)sender).Name.Equals("aliasBox"))
                    CheckUniqueAliasName(sender, e);

                BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                
                be.UpdateSource();
                e.Handled = true;
            }

        }

        #endregion

        private void CheckUniqueAliasName(object sender, RoutedEventArgs e)
        {
            PSMAttribute oldContext = (PSMAttribute)((TextBox)sender).DataContext;

            string text = ((TextBox)sender).Text;
            if (text == String.Empty)
                text = null;

            if (oldContext.AliasOrName == text)
                return;

            foreach (PSMAttribute p in oldContext.AttributeContainer.PSMAttributes)
            {
                if (p.AliasOrName == text)
                {
                    ErrDialog d = new ErrDialog();
                    d.SetText("Change alias of an attribute on a PSM diagram\n", "Name \"" + text + "\" cannot be used because it is already used in the collection.");
                    d.ShowDialog();

                    ((TextBox)sender).DataContext = null;
                    ((TextBox)sender).DataContext = oldContext;
                    return;
                }
            }

        }
    }
}
