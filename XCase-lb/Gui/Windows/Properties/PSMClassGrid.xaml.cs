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
    /// PSMClassGrid displays PSM Class properties in the Properties window
    /// </summary>
    public partial class PSMClassGrid
    {
        RenameElementConverter renameElement = null;
        RenameAliasConverter renameAlias = null;
        RenamePSMClassConverter renameClass = null;

        /// <summary>
        /// Initialization
        /// </summary>
        public PSMClassGrid()
        {
            InitializeComponent();
            attBoxes = new List<TextBox>();

            BrushRed.Color = Colors.Red;
            BrushBlack.Color = Colors.Black;
        }

        /// <summary>
        /// Saves all unsaved values in the grid
        /// </summary>
        public override void UpdateContent() 
        {
            if (classNameBox.IsFocused)
            {
                BindingExpression be = classNameBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
            }
            else
                if (elementNameBox.IsFocused)
                {
                    BindingExpression be = elementNameBox.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
        
            renameElement = null;
            renameClass = null;

            foreach (TextBox t in attBoxes)
            {
                if (t.IsFocused && renameAlias != null)
                {
                    CheckUniqueAliasName(t, null);

                    renameAlias.selectedAttribute = (PSMAttribute)(t.DataContext);
                    BindingExpression be = t.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
            
            }

          renameAlias = null;
          attBoxes.Clear();
        }

        /// <summary>
        ///  Displayes PSM class selected on the canvas
        /// </summary>
        /// <param name="c"></param>
        public void Display(XCaseViewBase c, MainWindow mainwin)
        {
            mv = mainwin;

            SelectedClassController = c.Controller;

            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "renameClass")
                {
                    renameClass = (RenamePSMClassConverter)ie.Value;
                    renameClass.Controller = SelectedClassController;
                }
                else
                if (ie.Key.ToString() == "renameAlias")
                {
                    renameAlias = (RenameAliasConverter)ie.Value;
                    renameAlias.Controller = SelectedClassController;
                }
                else
                if (ie.Key.ToString() == "renameElement")
                {
                    renameElement = (RenameElementConverter)ie.Value;
                    renameElement.Controller = SelectedClassController;
                }
            }

            classNameBox.DataContext = ((PSM_ClassController)SelectedClassController).Class;
            elementNameBox.DataContext = ((PSM_ClassController)SelectedClassController).Class;
            attBox.DataContext = ((PSM_ClassController)SelectedClassController).Class.AllPSMAttributes;
            representedClassBox.DataContext = ((PSM_ClassController) SelectedClassController).Class;

            appearance.SetElement(c.Controller.DiagramController, c.ViewHelper);
            appearance.DisableEdit();
        }

        #region Input validation

        private void CheckUniqueAliasName(object sender, RoutedEventArgs e)
        {
            PSMAttribute oldContext = (PSMAttribute)((TextBox)sender).DataContext;

            if (oldContext.Alias.Equals(((TextBox)sender).Text))
                return;

            if (oldContext.AttributeContainer != null)
            {
                foreach (PSMAttribute p in oldContext.AttributeContainer.PSMAttributes)
                {
                    if (p.Alias.Equals(((TextBox)sender).Text))
                    {
                        ErrDialog d = new ErrDialog();
                        d.SetText("Change alias of an attribute on a PSM diagram\n", "Name \"" + ((TextBox)sender).Text + "\" cannot be used because it is already used in the collection.");
                        d.ShowDialog();

                        ((TextBox)sender).DataContext = null;
                        ((TextBox)sender).DataContext = oldContext;
                        return;
                    }
                }
            
            }
            else
            if (oldContext.Class != null)
            {
                foreach (PSMAttribute p in oldContext.Class.PSMAttributes)
                {
                    if (p.Alias.Equals(((TextBox)sender).Text))
                    {
                        ErrDialog d = new ErrDialog();
                        d.SetText("Change alias of an attribute on a PSM diagram\n", "Name \"" + ((TextBox)sender).Text + "\" cannot be used because it is already used in the collection.");
                        d.ShowDialog();

                        ((TextBox)sender).DataContext = null;
                        ((TextBox)sender).DataContext = oldContext;
                        return;
                    }
                }
            }


        }

        #endregion

        #region Event handlers

        private void element_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            attBox.Width = e.NewSize.Width + 60;
            appearance.ExtendWidth(e.NewSize.Width);
            foreach (TextBox t in attBoxes)
                t.Width = e.NewSize.Width + 25;
        }

        private void PSMClassGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                elementNameBox.Width = e.NewSize.Width - 70;
                classNameBox.Width = e.NewSize.Width - 70;
                e.Handled = true;
            }
        }
 
        private void attBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (renameAlias != null)
                renameAlias.selectedAttribute = (PSMAttribute)(((TextBox)sender).DataContext);

            if (!attBoxes.Contains((TextBox)sender))
            {
                attBoxes.Add((TextBox)sender);
                ((TextBox)sender).Width = classNameBox.Width + 25;

            }

        }

        new private void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;

            // Confirmation of inserted value
            if (e.Key == Key.Enter && ((Control)sender).Focusable)
            {
                // Check new value correctness
                if (((TextBox)sender).Name.Equals("aliasBox"))
                    CheckUniqueAliasName(sender, e);

                BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();

                //((Control)sender).Focus();

                e.Handled = true;
            }

        }

        #endregion

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (representedClassBox.DataContext is PSMClass)
            {
                PIMClass representedClass = ((PSMClass)representedClassBox.DataContext).RepresentedClass;
                if (representedClass != null)
                    GoToPIMClass(representedClass);
            }
        }

        private SolidColorBrush BrushRed = new SolidColorBrush();
        private SolidColorBrush BrushBlack = new SolidColorBrush();
        private MainWindow mv;

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).Cursor = Cursors.Hand;
            ((TextBox)sender).Foreground = BrushRed;
            ((TextBox)sender).TextDecorations = TextDecorations.Underline;
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).Cursor = Cursors.Arrow;
            ((TextBox)sender).Foreground = BrushBlack;
            ((TextBox)sender).TextDecorations = null;
        }

        private void GoToPIMClass(PIMClass pimClass)
        {
            mv.DiagramTabManager.GoToPimClass(pimClass);   
        }
    }
}
