using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using XCase.Controller;
using XCase.Model;
using XCase.View.Controls;
using System.Windows.Media;
using XCase.Controller.Commands;
using XCase.Controller.Dialogs;


namespace XCase.Gui
{
    /// <summary>
    /// PIMClassGrid displays PIM Class properties in the Properties window
    /// </summary>
    public partial class PIMClassGrid
    {
        private RenamePIMClassConverter renamePIMClass =null;
        private RenameOperationConverter renameOperation = null;
        private RenameAttributeConverter renameAttribute = null;

        private List<TextBox> opBoxes = new List<TextBox>();

        private MainWindow mv;

        private SolidColorBrush BrushRed = new SolidColorBrush();
        private SolidColorBrush BrushBlack = new SolidColorBrush();

        /// <summary>
        /// Initialization
        /// </summary>
        public PIMClassGrid()
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
                CheckUniqueClassName(classNameBox, null);
                BindingExpression be = classNameBox.GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();
                renamePIMClass = null;
            }

            foreach (TextBox t in attBoxes)
            {
                if (renameAttribute != null && t.IsFocused)
                {
                    CheckUniqueAttributeName(t, null);
                    renameAttribute.selectedAttribute = (Property)(t.DataContext);
                    BindingExpression be = t.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
            }
            attBoxes.Clear();
            renameAttribute = null;

            foreach (TextBox t in opBoxes)
            {
                if (renameOperation != null && t.IsFocused)
                {
                    CheckUniqueOperationName(t, null);
                    renameOperation.selectedOperation = (Operation)(t.DataContext);
                    BindingExpression be = t.GetBindingExpression(TextBox.TextProperty);
                    be.UpdateSource();
                }
            }
            opBoxes.Clear();
            renameOperation = null;

            appearance.UpdateContent();
        }

        /// <summary>
        /// Displayes PIM class selected on the canvas
        /// </summary>
        /// <param name="c"></param>
        /// <param name="classtype"></param>
        /// <param name="mainwin"></param>
        public void Display(XCaseViewBase c, string classtype, MainWindow mainwin)
        {
            mv = mainwin;
            SelectedClassController = c.Controller;

            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                if (ie.Key.ToString() == "renamePIMClass")
                {
                    renamePIMClass = (RenamePIMClassConverter)ie.Value;
                    renamePIMClass.Controller = mv.CurrentProject.GetModelController();
                    renamePIMClass.selectedClass = (Class) c.Controller.Element;
                }
                else
                if (ie.Key.ToString() == "renameAttribute")
                {
                    renameAttribute = (RenameAttributeConverter)ie.Value;
                    renameAttribute.Controller = mv.CurrentProject.GetModelController();
                    renameAttribute.selectedClass = (Class) c.Controller.Element;
                }
                else
                if (ie.Key.ToString() == "renameOperation")
                {
                    renameOperation = (RenameOperationConverter)ie.Value;
                    renameOperation.Controller = mv.CurrentProject.GetModelController();
                }
            }

            mainLabel.Content = classtype + " Class";
          
            classNameBox.DataContext = ((ClassController)SelectedClassController).Class;
            attBox.DataContext = ((ClassController)SelectedClassController).Class.Attributes;
            opBox.DataContext = ((ClassController)SelectedClassController).Class.Operations;
            dcBox.DataContext = ((ClassController)SelectedClassController).Class.DerivedPSMClasses;

            appearance.Visibility = Visibility.Visible;
            appearance.SetElement(c.Controller.DiagramController, c.ViewHelper);
        }

        /// <summary>
        /// Displayes PIM class selected in the Navigator window
        /// </summary>
        /// <param name="p"></param>
        /// <param name="classtype"></param>
        /// <param name="mainwin"></param>
        public void Display(PIMClass p, string classtype, MainWindow mainwin)
        {
            mv = mainwin;

            IDictionaryEnumerator ie = grid.Resources.GetEnumerator();
            while (ie.MoveNext())
            {
                 if (ie.Key.ToString() == "renamePIMClass")
                 {
                     renamePIMClass = (RenamePIMClassConverter)ie.Value;
                     renamePIMClass.Controller = mv.CurrentProject.GetModelController();
                     renamePIMClass.selectedClass = p;
                 }
                 else
                 if (ie.Key.ToString() == "renameAttribute")
                 {
                     renameAttribute = (RenameAttributeConverter)ie.Value;
                     renameAttribute.Controller = mv.CurrentProject.GetModelController();
                     renameAttribute.selectedClass = p;
                 }
                 else
                 if (ie.Key.ToString() == "renameOperation")
                 {
                    renameOperation = (RenameOperationConverter)ie.Value;
                    renameOperation.Controller = mv.CurrentProject.GetModelController();
                 }

                mainLabel.Content = classtype + " Class";

                classNameBox.DataContext = p;
                attBox.DataContext = p.Attributes;
                opBox.DataContext = p.Operations;
                dcBox.DataContext = p.DerivedPSMClasses;

                appLabel.Visibility = Visibility.Hidden;
                appearance.Visibility = Visibility.Hidden;
            }
        }

        #region Event handlers

        private void opBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (renameOperation != null)
                renameOperation.selectedOperation = (Operation)(((TextBox)sender).DataContext);

            if (!opBoxes.Contains((TextBox)sender))
            {
                opBoxes.Add((TextBox)sender);
                ((TextBox)sender).Width = classNameBox.Width + 47;
            }
        }

        private void attBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (renameAttribute != null)
                renameAttribute.selectedAttribute = (Property)(((TextBox)sender).DataContext);

            if (!attBoxes.Contains((TextBox)sender))
            {
                attBoxes.Add((TextBox)sender);
                ((TextBox)sender).Width = classNameBox.Width + 25;
            }
        }

        private void TextBox_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).Cursor = Cursors.Hand;
            ((TextBox)sender).Foreground = BrushRed;
            ((TextBox)sender).TextDecorations = TextDecorations.Underline;
        }

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PSMClass derivedClass = (PSMClass)((Control)sender).DataContext;
            if (mv != null)
                mv.DiagramTabManager.ActivateDiagramWithElement(derivedClass.Diagram, derivedClass);
        }

        private void TextBox_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).Cursor = Cursors.Arrow;
            ((TextBox)sender).Foreground = BrushBlack;
            ((TextBox)sender).TextDecorations = null;
        }
 
        private void ClassGridBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 140)
            {
                classNameBox.Width = e.NewSize.Width - 70;

                appearance.ExtendWidth(e.NewSize.Width -70);
                attBox.Width = e.NewSize.Width -10;
                opBox.Width = e.NewSize.Width - 10;
                dcBox.Width = e.NewSize.Width - 10;
                foreach (TextBox t in attBoxes)
                    t.Width = e.NewSize.Width - 45;
                foreach (TextBox o in opBoxes)
                    o.Width = e.NewSize.Width - 23;
            }

            e.Handled = true;
        }

        new private void KeyPressed_Down(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Left)
                e.Handled = true;

            // Confirmation of inserted value
            if (e.Key == Key.Enter && ((Control)sender).Focusable)
            {
                // Check new value correctness
                if (((TextBox)sender).Name.Equals("attNameBox"))
                    CheckUniqueAttributeName(sender, e);
                else
                    if (((TextBox)sender).Name.Equals("opNameBox"))
                        CheckUniqueOperationName(sender, e);
                    else
                        if (((TextBox)sender).Name.Equals("classNameBox"))
                            CheckUniqueClassName(sender, e);

                BindingExpression be = ((Control)sender).GetBindingExpression(TextBox.TextProperty);
                be.UpdateSource();

                //((Control)sender).Focus();

                e.Handled = true;
            }

        }

        private void TextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (((TextBox)sender).Name.Equals("derivedClassBox"))
            {
                object oldContext = ((TextBox)sender).DataContext;
                ((TextBox)sender).DataContext = null;
                ((TextBox)sender).DataContext = oldContext;
            }
        }

        private void attNameBox_Initialized(object sender, EventArgs e)
        {
            if (!attBoxes.Contains((TextBox)sender))
            {
                attBoxes.Add((TextBox)sender);
                ((TextBox)sender).Width = classNameBox.Width + 25;
            }

        }

        #endregion

        #region Input validation

        /// <summary>
        /// Checks whether the new class name is unique
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUniqueClassName(object sender, RoutedEventArgs e)
        {
            Class oldContext = (Class)((TextBox)sender).DataContext;

            if (oldContext.Name.Equals(((TextBox)sender).Text))
                return;

            foreach (Class c in oldContext.Schema.Model.Classes)
            {
                if (c.Name.Equals(((TextBox)sender).Text))
                {
                    // Refuse new value

                    ErrDialog d = new ErrDialog();
                    d.SetText("Rename element\n", "Name \""+((TextBox)sender).Text+"\" cannot be used because it is already used in the collection.");
                    d.ShowDialog();

                    ((TextBox)sender).DataContext = null;
                    ((TextBox)sender).DataContext = oldContext;
                    return;
                }
            }
        }

        /// <summary>
        /// Checks whether the new attribute name is unique
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUniqueAttributeName(object sender, RoutedEventArgs e)
        {
            Property oldContext = (Property)((TextBox)sender).DataContext;

            if (oldContext.Name.Equals(((TextBox)sender).Text))
                return;

            foreach (Property p in oldContext.Class.Attributes)
            {
                if (p.Name.Equals(((TextBox)sender).Text))
                {
                    // Refuse new value

                    ErrDialog d = new ErrDialog();
                    d.SetText("Update property\n", "Name \"" + ((TextBox)sender).Text + "\" cannot be used because it is already used in the collection.");
                    d.ShowDialog();

                    ((TextBox)sender).DataContext = null;
                    ((TextBox)sender).DataContext = oldContext;
                    return;
                }
            }
        }

        /// <summary>
        /// Checks whether the new operation name is unique
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckUniqueOperationName(object sender, RoutedEventArgs e)
        {
            Operation oldContext = (Operation)((TextBox)sender).DataContext;

            if (oldContext.Name.Equals(((TextBox)sender).Text))
                return;

            foreach (Operation p in oldContext.Class.Operations)
            {
                if (p.Name.Equals(((TextBox)sender).Text))
                {
                    // Refuse new value

                    ErrDialog d = new ErrDialog();
                    d.SetText("Update operation\n", "Name \"" + ((TextBox)sender).Text + "\" cannot be used because it is already used in the collection.");
                    d.ShowDialog();

                    ((TextBox)sender).DataContext = null;
                    ((TextBox)sender).DataContext = oldContext;
                    return;
                }
            }
        }


        #endregion

    }
}
